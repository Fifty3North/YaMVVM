# F3N.YaMVVM
![Nuget](https://img.shields.io/nuget/v/F3N.YaMVVM.svg)
![Azure DevOps builds](https://img.shields.io/azure-devops/build/andy0505/3270ed0d-e050-46bb-be0a-077a5b7e8f5a/2.svg)

Yet another Model, View, ViewModel framework for Xamarin Forms.

## Why?

Microsoft has spent time making Xamarin better at supporting MVVM but its still not there yet.

Existing alternatives are quite large and complex and are not tailored specifically to Xamarin Forms.

We needed something that was lightweight and would remove the boilerplate code needed for working with MVVM in Xamarin Forms.

## What does it do?

This was created to: 
* manage the lifecycle of view models when navigating between pages
* remove the requirement to remember whether a page was opened normally or as a modal page when popping
* allow viewmodel to know when page was re-opened, e.g. when appearing after user clicked back
* call DisplayAlert from view model

## Works well with:
[F3N.Hoard](https://github.com/Fifty3North/Hoard) 
> Cross platform storage for native apps used in-house at Fifty3North.

## Getting started

Install NuGet package `F3N.YaMVVM` into Xamarin Forms project

`App.xaml.cs` inherits from `YamvvmApp`
In XAML change App to:
```
<yamvvm:YamvvmApp xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:yamvvm="clr-namespace:F3N.YaMVVM.App;assembly=F3N.YaMVVM"
...
```

#### View Models
Page view models inherit from `PageViewModel`

Components or sub views inherit from `YamvvmViewModel`

#### Pages
Pages inherit from one of:
* `YamvvmNavigationPage`
* `YamvvmPage`

```
<yamvvm:YamvvmPage xmlns="http://xamarin.com/schemas/2014/forms"
    xmlns:yamvvm="clr-namespace:F3N.YaMVVM.Views;assembly=F3N.YaMVVM"
    ...
```

Set main page using one of:
* `ViewModelNavigation.SetTabbedMainPage<TabbedPage>();`
* `ViewModelNavigation.SetMainPage<MainPage>();`
* `ViewModelNavigation.SetMasterDetailPage<MasterPage,DetailPage>();`

## Navigation

Use `PushPage` and `PushModalPage` as required

USe `PopPage` in your ViewModel without worrying if the current page was opened using modal or not.

## Lifecycle

First time view model initialised you can override:
```
public override async Task Initialise() { ... }
```

Subsequent times page appears use:
```
public override async Task OnReappearing() { ... }
```

When page is popped dispose of any objects using:
```
public override async Task Destroy()
```