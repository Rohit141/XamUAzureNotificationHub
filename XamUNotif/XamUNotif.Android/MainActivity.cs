﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Firebase.Messaging;
using Firebase.Iid;
using Android.Util;
using Android.Gms.Common;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace XamUNotif.Droid
{
	[Activity(Label = "XamUNotif", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);
			LoadApplication(new App());

			// In a "real" app you will have to deal with it if services are unavailable!
			IsPlayServicesAvailable();

#if DEBUG
			// Force refresh of the token. If we redeploy the app, no new token will be sent but the old one will
			// be invalid.
			Task.Run(() =>
			{
				// This may not be executed on the main thread.
				FirebaseInstanceId.Instance.DeleteInstanceId();
				Console.WriteLine("Forced token: " + FirebaseInstanceId.Instance.Token);
			});
#endif
		}

		public bool IsPlayServicesAvailable()
		{
			int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
			if (resultCode != ConnectionResult.Success)
			{
				if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
				{
					// In a real project you can give the user a chance to fix the issue.
					Console.WriteLine($"Error: {GoogleApiAvailability.Instance.GetErrorString(resultCode)}");
				}
				else
				{
					Console.WriteLine("Error: Play services not supported!");
					Finish();
				}
				return false;
			}
			else
			{
				Console.WriteLine("Play Services available.");
				return true;
			}
		}
	}

	// This service handles the device's registration with FCM.
	[Service]
	[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
	public class MyFirebaseIIDService : FirebaseInstanceIdService
	{
		public override void OnTokenRefresh()
		{
			var refreshedToken = FirebaseInstanceId.Instance.Token;
			Console.WriteLine($"Token received: {refreshedToken}");
			SendRegistrationToServer(refreshedToken);
		}

		void SendRegistrationToServer(string token)
		{
			// We'll do this later :-)
		}
	}

	// This service is used if app is in the foreground and a message is received.
	[Service]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class MyFirebaseMessagingService : FirebaseMessagingService
	{
		public override void OnMessageReceived(RemoteMessage message)
		{
			base.OnMessageReceived(message);

			Console.WriteLine("Received: " + message);

			// Android supports different message payloads. To use the code below it must be something like this (you can paste this into Azure test send window):
			// {
			//   "notification" : {
			//      "body" : "The body",
			//                 "title" : "The title",
			//                 "icon" : "myicon
			//   }
			// }
			try
			{
				var msg = message.GetNotification().Body;
				var msgReceiver = (AndroidMessageReceiver)DependencyService.Get<IMessageReceiver>(DependencyFetchTarget.GlobalInstance);
				msgReceiver.Handle(msg);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error extracting message: " + ex);
			}
		}
	}
}
