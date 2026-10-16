# Intelmeter fork of Gurux.DLMS.UI.Net

This is an unofficial community fork, maintained under the Intelmeter GitHub account. It is not an official Gurux release and does not imply Gurux endorsement. Original Gurux attribution is retained below.

License: **GPL-2.0-only**. The fork's contributions are distributed under the same license; original Gurux and third-party notices remain applicable. See [LICENSE](LICENSE), [MODIFICATIONS.txt](MODIFICATIONS.txt), [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt), and [RELEASE-COMPLIANCE.txt](RELEASE-COMPLIANCE.txt).

Build on Windows with the .NET 10 SDK. Clone both Intelmeter repositories into sibling directories named GXDLMSDirector and Gurux.DLMS.UI.Net, then run from their parent directory:

    dotnet build GXDLMSDirector/Development/GXDLMSDirector46.csproj -c Release -t:Rebuild

The protocol library remains the official Gurux.DLMS 9.0.2609.2201 NuGet package. This GPL dependency is not exempt from redistribution obligations merely because it is unmodified.

---

See An [Gurux](http://www.gurux.org/ "Gurux") for an overview.

Join the Gurux Community or follow [@Gurux](https://twitter.com/guruxorg "@Gurux") for project updates.

Gurux.DLMS library is a high-performance .NET component that helps you to read you DLMS/COSEM compatible electricity, gas or water meters. We have try to make component so easy to use that you do not need understand protocol at all.

For more info check out [Gurux.DLMS](http://www.gurux.fi/index.php?q=Gurux.DLMS "Gurux.DLMS").


Gurux.DLMS.UI
=========================== 
Purpose of Gurux.DLMS.UI is help to create user interface for DLMS applications.
First you need to create Panel where UI is shown. In this example we name it as UIPanel.
 You can create DLMS views at the run time,
but usually it's better to create them all and update panel when user selects next COSEM object.
You can create all UI objects like this.

```csharp
//All UI objects are save on views dictionary.
Dictionary<Type, IGXDLMSView> views = new Dictionary<Type, IGXDLMSView>();

foreach (Type type in typeof(GXDLMSDataView).Assembly.GetTypes())
{
    GXDLMSViewAttribute[] att = (GXDLMSViewAttribute[])type.GetCustomAttributes(typeof(GXDLMSViewAttribute), true);
    if (!type.IsInterface && typeof(IGXDLMSView).IsAssignableFrom(type))
    {
        IGXDLMSView view = Activator.CreateInstance(type) as IGXDLMSView;
        Form f = view as Form;
        f.TopLevel = false;
        f.TopMost = false;
        f.FormBorderStyle = FormBorderStyle.None;
        f.Dock = DockStyle.Fill;
        f.Width = UIPanel.Width;
        f.Height = UIPanel.Height;
        views.Add(att[0].DLMSType, view);
    }
}

``` 
When we want to show UI for selected COSEM object we can do it like this:
```csharp
//Clear old view.
foreach (Control it in UIPanel.Controls)
{
    it.Hide();
}
UIPanel.Controls.Clear();
//Select new COSEM object. In this example we select clock.
GXDLMSClock clock = new GXDLMSClock();
IGXDLMSView selectedView = views[clock];
selectedView.Target = clock;
GXDlmsUi.UpdateProperties(clock, selectedView);
//Add DLMS COSEM view to UI panel.
UIPanel.Controls.Add((Form)selectedView);
Show UI.
((Form)selectedView).Show();

``` 