﻿using System;

using Xamarin.Forms;

namespace Bit.CSharpClient.Controls.Samples
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void DateTimePicker_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BitDateTimePickerSampleView());
        }
    }
}
