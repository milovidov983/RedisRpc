﻿using RedisRpc;
using RedisRpc.Interfaces;
using RedisRpc.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCC = RedisCalc.Contracts;

// docker run --name some-redis -d -p 6379:6379 redis:5.0.5

namespace RedisTestClient {
	class Program {
		private static IRedisHub rpcService;


		static async Task Main(string[] args) {
			Console.Title = "RedisTest - CLIENT";

			Console.WriteLine("Подключение к redis...");
			rpcService = new RedisHub(new Options {
				HostsCollection = new List<(string Host, int Port)> {	(Host:"localhost", Port:6379)	}
			});

			Console.WriteLine("Подключено.");
			int num = 1;
			while (true) {
				Console.WriteLine("Отправка запроса...");

				var result = await rpcService.GetResponse<RCC.Add.Response, RCC.Add.Request>(
					RCC.Add.Topic,
					new RCC.Add.Request {
						A = num
				});
				Console.WriteLine($"Ответ от микросервиса {result.Results}");

				Console.Write("Введите число: ");
				num = Convert.ToInt32(Console.ReadLine());

				if (num == -1) {
					break;
				}
			}

		}


	}
}
