﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using DevHome.Common.Contracts;
using DevHome.Common.Extensions;
using DevHome.Common.Helpers;
using DevHome.Common.Services;
using DevHome.Dashboard.ViewModels;
using DevHome.Models;
using DevHome.Services;
using DevHome.Settings.ViewModels;
using DevHome.SetupFlow.Utilities;
using DevHome.Telemetry;
using DevHome.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.DevHome.SDK;
using Windows.System;

namespace DevHome.Views;

public sealed partial class WhatsNewPage : Page
{
    public Uri DevDrivePageKeyUri { get; private set; } = new ("ms-settings:disksandvolumes");

    public Uri DevDriveLearnMoreLinkUri { get; private set; } = new ("https://go.microsoft.com/fwlink/?linkid=2236041");

    public string DevDriveLinkResourceKey { get; private set; } = "WhatsNewPage_DevDriveCard/Link";

    public WhatsNewViewModel ViewModel
    {
        get;
    }

    public WhatsNewPage()
    {
        ViewModel = Application.Current.GetService<WhatsNewViewModel>();
        InitializeComponent();
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await Application.Current.GetService<ILocalSettingsService>().SaveSettingAsync(WellKnownSettingsKeys.IsNotFirstRun, true);

        var whatsNewCards = FeaturesContainer.Resources
            .Where((item) => item.Value.GetType() == typeof(WhatsNewCard))
            .Select(card => card.Value as WhatsNewCard)
            .OrderBy(card => card?.Priority ?? 0);

        foreach (var card in whatsNewCards)
        {
            if (card is null)
            {
                continue;
            }

            // When the Dev Drive feature is not enabled don't show the learn more uri link, but instead move the learn more text into the button content.
            if (card!.PageKey!.Equals(DevDrivePageKeyUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
            {
                if (!DevDriveUtil.IsDevDriveFeatureEnabled)
                {
                    card.Button = Application.Current.GetService<IStringResource>().GetLocalized(DevDriveLinkResourceKey);
                    card.ShouldShowLink = false;
                }
            }

            ViewModel.AddCard(card);
        }
    }

    private void MachineConfigButton_Click(object sender, RoutedEventArgs e)
    {
        var navigationService = Application.Current.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(DevHome.SetupFlow.ViewModels.SetupFlowViewModel).FullName!);
    }

    private async void Button_ClickAsync(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;

        if (btn?.DataContext is not string pageKey)
        {
            return;
        }

        if (pageKey.Equals(DevDrivePageKeyUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
        {
            // Only launch the disks and volumes settings page when the Dev Drive feature is enabled.
            // Otherwise redirect the user to the Dev Drive support page to learn more about the feature.
            await Launcher.LaunchUriAsync(DevDriveUtil.IsDevDriveFeatureEnabled ? DevDrivePageKeyUri : DevDriveLearnMoreLinkUri);
        }
        else
        {
            var navigationService = Application.Current.GetService<INavigationService>();
            navigationService.NavigateTo(pageKey!);
        }
    }

    public static class MyHelpers
    {
        public static Type GetType(object ele)
        {
            return ele.GetType();
        }
    }
}
