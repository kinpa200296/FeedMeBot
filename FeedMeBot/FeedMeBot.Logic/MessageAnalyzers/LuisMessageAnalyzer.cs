using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FeedMeBot.Logic
{
    public class LuisMessageAnalyzer : IMessageAnalyzer
    {
        private LuisService _luisService;


        public LuisMessageAnalyzer(string appId, string apiKey, string hostname)
        {
            _luisService = new LuisService(new LuisModelAttribute(appId, apiKey, domain: hostname));
        }


        public async Task<string> GetResponse(string message, Order currentOrder)
        {
            try
            {
                var request = new LuisRequest(message);
                var res = await _luisService.QueryAsync(request, CancellationToken.None);
                return HandleResult(res, currentOrder);
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }


        private string HandleResult(LuisResult result, Order currentOrder)
        {
            var score = result.TopScoringIntent.Score;
            switch (result.TopScoringIntent.Intent)
            {
                case "None":
                    return HandleNoneEn();
                case "None.Ru":
                    return HandleNoneRu();
                case "Greeting.En":
                    return score > 0.7 ? HandleGreetingEn() : HandleNoneEn();
                case "Greeting.Ru":
                    return score > 0.6 ? HandleGreetingRu() : HandleNoneRu();
                case "ShowMenu.En":
                    return score > 0.8 ? HandleShowMenuEn() : HandleNoneEn();
                case "ShowMenu.Ru":
                    return score > 0.6 ? HandleShowMenuRu() : HandleNoneRu();
                case "ShowOrder.En":
                    return score > 0.8 ? HandleShowOrderEn(currentOrder) : HandleNoneEn();
                case "ShowOrder.Ru":
                    return score > 0.7 ? HandleShowOrderRu(currentOrder) : HandleNoneRu();
                case "Checkout.En":
                    return score > 0.8 ? HandleCheckoutEn(currentOrder) : HandleNoneEn();
                case "Checkout.Ru":
                    return score > 0.6 ? HandleCheckoutRu(currentOrder) : HandleNoneRu();
                case "Order.En":
                    return score > 0.8 ? HandleOrderEn(result, currentOrder) : HandleNoneEn();
                case "Order.Ru":
                    return score > 0.8 ? HandleOrderRu(result, currentOrder) : HandleNoneRu();


                default:
                    return HandleFault();
            }
        }

        private string HandleFault()
        {
            return "Something went wrong. I can't understand your request";
        }

        private string HandleNoneEn()
        {
            return "I can't understand your request";
        }

        private string HandleNoneRu()
        {
            return "Я не могу понять ваш запрос";
        }

        private string HandleGreetingEn()
        {
            return "Hello, what would you like to order?";
        }

        private string HandleGreetingRu()
        {
            return "Доброго времени суток, что будете заказывать?";
        }

        private string HandleShowMenuEn()
        {
            return "We offer pancakes with different fillings: mustard, ketchup, cheese, caviar, jam, mayo, ham, crab sticks, pineapple";
        }

        private string HandleShowMenuRu()
        {
            return "Мы предлагаем блины с различными начинками: горчица, кетчуп, сыр, икра, варенье, майонез, крабовые палочки, ананас, сгущенка";
        }

        private string HandleShowOrderEn(Order currentOrder)
        {
            var sb = new StringBuilder();
            if (currentOrder.Dishes.Count > 0)
            {
                sb.AppendLine("Your order:");
                foreach (var dish in currentOrder.Dishes)
                {
                    sb.AppendLine(dish.ToString());
                }
            }
            else
            {
                sb.AppendLine("You haven't ordered anything yet :(");
            }
            return sb.ToString();
        }

        private string HandleShowOrderRu(Order currentOrder)
        {
            var sb = new StringBuilder();
            if (currentOrder.Dishes.Count > 0)
            {
                sb.AppendLine("Ваш заказ:");
                foreach (var dish in currentOrder.Dishes)
                {
                    sb.AppendLine(dish.ToString());
                }
            }
            else
            {
                sb.AppendLine("Вы пока ничего не заказали :(");
            }
            return sb.ToString();
        }

        private string HandleCheckoutEn(Order currentOrder)
        {
            if (currentOrder.Dishes.Count > 0)
            {
                currentOrder.Dishes.Clear();
                return $"Thanks for your order! One day you will be able to pick it up :). Your order id: {Guid.NewGuid()}";
            }
            return "You haven't ordered anything yet :(";
        }

        private string HandleCheckoutRu(Order currentOrder)
        {
            if (currentOrder.Dishes.Count > 0)
            {
                currentOrder.Dishes.Clear();
                return $"Спасибо за заказ! Однажды вы сможете его забрать :). Идентификатор вашего заказа: {Guid.NewGuid()}";
            }
            return "Вы пока ничего не заказали :(";
        }

        private string HandleOrderEn(LuisResult result, Order currentOrder)
        {
            var dishes = ParseDishes(result);
            if (dishes.Count > 0)
            {
                currentOrder.Dishes.AddRange(dishes);
                var sb = new StringBuilder();
                sb.AppendLine("Your have ordered:");
                foreach (var dish in dishes)
                {
                    sb.AppendLine(dish.ToString());
                }
                return sb.ToString();
            }
            return "You wanted to order something. But I can't understand what :(";
        }

        private string HandleOrderRu(LuisResult result, Order currentOrder)
        {
            var dishes = ParseDishes(result);
            if (dishes.Count > 0)
            {
                currentOrder.Dishes.AddRange(dishes);
                var sb = new StringBuilder();
                sb.AppendLine("Вы заказали:");
                foreach (var dish in dishes)
                {
                    sb.AppendLine(dish.ToString());
                }
                return sb.ToString();
            }
            return "Вы хотели что-то заказать. Но я не понял что :(";
        }

        private List<Dish> ParseDishes(LuisResult result)
        {
            var dishes = new List<Dish>();
            if (result.CompositeEntities != null)
            {
                foreach (var entity in result.CompositeEntities)
                {
                    if (entity.Children.Count(c => c.Type == "DishBase") == 1)
                    {
                        var dish = new Dish();
                        foreach (var child in entity.Children)
                        {
                            if (child.Type == "DishBase")
                            {
                                dish.Base = child.Value;
                            }
                            else
                            {
                                dish.Fillings.Add(child.Value);
                            }
                        }
                        dishes.Add(dish);
                    }
                }
            }
            return dishes;
        }
    }
}
