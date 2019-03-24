using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections;
using UnityEngine;
using Smod2.API;
using System.Diagnostics;
using System;
using System.Linq;
using Smod2.Commands;

namespace BadBoiDetector
{
	[PluginDetails(
		author = "Citro",
		name = "Bad Boi Detector",
		description = "Help catch those bad bois",
		id = "com.citro.badboidetector",
		version = "1.3",
		SmodMajor = 3,
		SmodMinor = 0,
		SmodRevision = 0
		)]
	class Plugin : Smod2.Plugin
	{ 
		public override void OnDisable()
		{
		}   
	
		public override void OnEnable()
		{
		   if(!this.GetConfigBool("bbd_disable"))
			this.Info("Bad Boi Detector has been loaded.");
		}

		public override void Register()
		{

			this.AddConfig(new Smod2.Config.ConfigSetting("bbd_disable", false, Smod2.Config.SettingType.BOOL, true, "Disables Bad Boi Detector plugin."));
			this.AddConfig(new Smod2.Config.ConfigSetting("bbd_sendsensitiveinfo", true, Smod2.Config.SettingType.BOOL, true, "Whenether or not to include sensitive information in requests."));
			if (!this.GetConfigBool("bbd_disable"))
			this.AddEventHandlers(new ServerEventHandler(this), Priority.Normal);
			
		}
	}
}
