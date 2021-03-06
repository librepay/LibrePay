﻿using System;
using System.Threading.Tasks;
using Android.Widget;
using LibrePay.Droid.Services;
using LibrePay.Interfaces.Devices;
using Xamarin.Forms;
using Application = Android.App.Application;

[assembly: Dependency(typeof(AndroidMessageDisplayer))]

namespace LibrePay.Droid.Services
{
    public class AndroidMessageDisplayer : IMessageDisplayer
    {
        public Task ShowMessageAsync(string text)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                var toast = Toast.MakeText(Application.Context, text, ToastLength.Short);
                toast.Show();
                tcs.SetResult(true);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }

            return tcs.Task;
        }
    }
}
