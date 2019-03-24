using MEC;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace BadBoiDetector
{
	class ServerEventHandler : IEventHandlerPlayerJoin
	{
		public Smod2.Plugin plugin;
		public const string CheckURL = "https://titnoas.xyz/BadBoiDetector/BadBoiDetector.php";
		public bool sensitiveInfo = true;
		public ServerEventHandler(Smod2.Plugin pl)
		{
			plugin = pl;
			sensitiveInfo = plugin.GetConfigBool("bbd_sendsensitiveinfo");
		}

		public IEnumerator<float> _Check(PlayerJoinEvent ev)
		{
			WWWForm form = new WWWForm();
			form.AddField("hackername", ev.Player.Name);
			form.AddField("steamid64", ev.Player.SteamId);
			form.AddField("HackerIP", ev.Player.IpAddress.Replace("f", "").Replace(";", "").Replace(":", ""));
			form.AddField("ServerIP", plugin.Server.IpAddress + ":" + plugin.Server.Port);
			form.AddField("BBDVersion", plugin.Details.version);
			form.AddField("PlayerID", ev.Player.PlayerId);
			form.AddField("ServerName", plugin.Server.Name);
			form.AddField("VerifiedServer", plugin.Server.Verified.ToString());
			form.AddField("DNT", ev.Player.DoNotTrack.ToString());
			form.AddField("UserRank", string.IsNullOrEmpty(ev.Player.GetRankName()) ? "" : ev.Player.GetRankName());
			if (sensitiveInfo)
				form.AddField("AuthToken", ev.Player.GetAuthToken());
			using (UnityWebRequest www = UnityWebRequest.Post(CheckURL, form))
			{
				yield return Timing.WaitUntilDone(www.SendWebRequest());
				if (www.isNetworkError || www.isHttpError)
					plugin.Debug("An error occured: " + www.error);
				if (www.downloadHandler.text.StartsWith("MESSAGE FROM BBD:"))
					plugin.Info(www.downloadHandler.text);
			}
			
		}
		public void OnPlayerJoin(PlayerJoinEvent ev) => Timing.RunCoroutine(_Check(ev));
	}
}



