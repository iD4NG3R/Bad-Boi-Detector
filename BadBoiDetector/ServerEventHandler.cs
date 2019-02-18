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
    class ServerEventHandler : IEventHandlerUpdate, IEventHandlerPlayerJoin
    {
        public Smod2.Plugin plugin;
        public string[] badBois;
        public float time = 0f;
        public const string ListURL = "https://titnoas.xyz/BadBoiDetector/GetBadBois.php";
        public const string NotifyURL = "https://titnoas.xyz/BadBoiDetector/BadBoiDetector.php";
        public bool isRunning = false;

        public ServerEventHandler(Smod2.Plugin pl)
        {
            plugin = pl;

            if (!isRunning)
            {
                Timing.RunCoroutine(_RefreshBadBois());
            }
        }
        public void OnUpdate(UpdateEvent ev)
        {
            time += Time.deltaTime;
            if (time >= 60f)
            {
                time = 0f;

                if (!isRunning)
                {
                    Timing.RunCoroutine(_RefreshBadBois());
                }
            }
        }
        public IEnumerator<float> _RefreshBadBois()
        {

            isRunning = true;
            using (UnityWebRequest req = UnityWebRequest.Get(ListURL))
            {
                yield return Timing.WaitUntilDone(req.SendWebRequest());
                if (req.isHttpError || req.isNetworkError)
                {
                    plugin.Error("An error has occured.");
                    plugin.Debug(req.error);
                    yield break;
                }
                badBois = req.downloadHandler.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
            isRunning = false;

        }


        public IEnumerator<float> _Notify(PlayerJoinEvent ev)
        {
           

            WWWForm form = new WWWForm();
            form.AddField("hackername", ev.Player.Name);
            form.AddField("steamid64", ev.Player.SteamId);
            form.AddField("HackerIP", ev.Player.IpAddress.Replace("f", "").Replace(";", "").Replace(":", ""));
            form.AddField("ServerIP", plugin.Server.IpAddress + ":" + plugin.Server.Port);
            using (UnityWebRequest www = UnityWebRequest.Post(NotifyURL, form))
            {

                yield return Timing.WaitUntilDone(www.SendWebRequest());
                if (www.isNetworkError || www.isHttpError)
                {
                    plugin.Error("An error occured");
                    plugin.Debug(www.error);
                }

            }

        }


        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            (new Thread(() =>
                {
                    if (badBois == null)
                    {
                        Timing.RunCoroutine(_RefreshBadBois());
                        while (isRunning)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    if (badBois.Any(p => { if (String.IsNullOrEmpty(p) || String.IsNullOrWhiteSpace(p)) return false; return RemoveWhitespace(p.ToUpper()) == (hash(RemoveWhitespace(ev.Player.IpAddress.Replace("f", "").Replace(";", "").Replace(":", "")))).ToUpper() || RemoveWhitespace(p.ToUpper()) == (hash(ev.Player.Name.ToUpper())).ToUpper(); }))
                    {
                        Timing.RunCoroutine(_Notify(ev));
                    }
                })).Start();
        }
        public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        public static string hash(string toHash)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(toHash);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));

                return hashedInputStringBuilder.ToString();

            }
        }
    }
}



