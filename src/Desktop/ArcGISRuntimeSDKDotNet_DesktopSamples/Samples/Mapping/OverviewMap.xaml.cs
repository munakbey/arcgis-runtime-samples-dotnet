﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Linq;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates displaying an overview map to indicate the extent of the parent map.
    /// </summary>
    /// <title>Overview Map</title>
    /// <category>Mapping</category>
    public partial class OverviewMap : UserControl
    {
        /// <summary>Construct Overview Map sample control</summary>
        public OverviewMap()
        {
            InitializeComponent();

			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-5, 20, 50, 65, SpatialReferences.Wgs84));

            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        private async void MyMapView_ExtentChanged(object sender, System.EventArgs e)
        {
            // Update overview map graphic
			Graphic g = overviewOverlay.Graphics.FirstOrDefault();
            if (g == null) //first time
            {
                g = new Graphic();
				overviewOverlay.Graphics.Add(g);
            }
            g.Geometry = MyMapView.Extent;

            // Adjust overview map scale
            await overviewMap.SetViewAsync(MyMapView.Extent.GetCenter(), MyMapView.Scale * 15);
        }
    }
}
