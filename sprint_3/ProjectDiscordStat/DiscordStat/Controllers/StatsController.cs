﻿using DiscordStats.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RestSharp;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using System.Net;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;
using DiscordStats.DAL.Abstract;
using DiscordStats.ViewModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordStats.ViewModels;

namespace DiscordStats.Controllers
{
    [Authorize]
    public class StatsController : Controller
    {

        private readonly IDiscordUserAndUserWebSiteInfoRepository _userRepository;
        private readonly IPresenceRepository _presenceRepository;
        private readonly ILogger<ApiController> _logger;
        private readonly IDiscordService _discord;
        private readonly IServerRepository _serverRepository;
        private readonly IChannelRepository _channelRepository;
        private readonly IMessageInfoRepository _messageInfoRepository;
        private readonly IVoiceStateRepository _voiceStateRepository;

        public StatsController(ILogger<ApiController> logger, IDiscordUserRepository discordUserRepo, IPresenceRepository presenceRepository, IDiscordService discord, IServerRepository serverRepository, IChannelRepository channelRepository, IMessageInfoRepository messageInfoRepository, IVoiceStateRepository voiceStateRepository)
        {
            _logger = logger;
            _userRepository = userRepo;
            _presenceRepository = presenceRepository;
            _discord = discord;
            _serverRepository = serverRepository;
            _channelRepository = channelRepository;
            _messageInfoRepository = messageInfoRepository;
            _voiceStateRepository = voiceStateRepository;
        }

        public IActionResult ServerStats(string ServerId)
        {
            if (_serverRepository.GetAll().Any(c => c.Id == ServerId))
                return View((object)ServerId);
            else
                return View();
        }

        [HttpGet]
        public IActionResult GetMessageInfoFromDatabase(string ServerId)
        {
            return Json(_messageInfoRepository.GetAll().Where(s => s.ServerId == ServerId).ToList());
        }

        [HttpGet]
        public IActionResult GetPresencesFromDatabase(string ServerId, string GameName)
        {
            return Json(_presenceRepository.GetAll().Where(s => s.ServerId == ServerId && s.Name == GameName).ToList());
        }

        [HttpGet]
        public IActionResult GetUsersFromDatabase(string ServerId)
        {
            return Json(_discordUserRepository.GetAll().Where(s => s.Servers == ServerId).ToList());
        }

        [HttpGet]
        public IActionResult GetVoiceStatesFromDatabase(string ServerId)
        {
            return Json(_voiceStateRepository.GetAll().Where(s => s.ServerId == ServerId).ToList());
        }
    }
}
