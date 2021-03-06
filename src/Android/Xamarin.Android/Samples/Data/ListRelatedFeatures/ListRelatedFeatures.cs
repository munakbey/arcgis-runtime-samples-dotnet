// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ArcGISRuntime.Samples.ListRelatedFeatures
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List related features",
        "Data",
        "This sample demonstrates how to query features related to an identified feature.",
        "Click on a feature to identify it. Related features will be listed in the window above the map.")]
    public class ListRelatedFeatures : Activity
    {
        // URL to the web map
        private readonly Uri _mapUri =
            new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=dcc7466a91294c0ab8f7a094430ab437");

        // Reference to the feature layer
        private FeatureLayer _myFeatureLayer;

        // Hold a reference to the map view
        private MapView _myMapView;

        // Hold a reference to the ListView
        private ListView _myDisplayList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "List related features";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Create the portal item from the URL to the webmap
                PortalItem alaskaPortalItem = await PortalItem.CreateAsync(_mapUri);

                // Create the map from the portal item
                Map myMap = new Map(alaskaPortalItem);

                // Add the map to the mapview
                _myMapView.Map = myMap;

                // Wait for the map to load
                await myMap.LoadAsync();

                // Get the feature layer from the map
                _myFeatureLayer = (FeatureLayer)myMap.OperationalLayers.First();

                // Update the selection color
                _myMapView.SelectionProperties.Color = Color.Yellow;

                // Listen for GeoViewTapped events
                _myMapView.GeoViewTapped += MyMapViewOnGeoViewTapped;
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private async void MyMapViewOnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing feature selection and results list
            _myFeatureLayer.ClearSelection();
            _myDisplayList.Adapter = null;

            try
            {
                // Identify the tapped feature
                IdentifyLayerResult results = await _myMapView.IdentifyLayerAsync(_myFeatureLayer, e.Position, 10, false);

                // Return if there are no results
                if (results.GeoElements.Count < 1) { return; }

                // Get the first result
                ArcGISFeature myFeature = (ArcGISFeature)results.GeoElements.First();

                // Select the feature
                _myFeatureLayer.SelectFeature(myFeature);

                // Get the feature table for the feature
                ArcGISFeatureTable myFeatureTable = (ArcGISFeatureTable)myFeature.FeatureTable;

                // Query related features
                IReadOnlyList<RelatedFeatureQueryResult> relatedFeaturesResult = await myFeatureTable.QueryRelatedFeaturesAsync(myFeature);

                // Create a list to hold the formatted results of the query
                List<String> queryResultsForUi = new List<string>();

                // For each query result
                foreach (RelatedFeatureQueryResult result in relatedFeaturesResult)
                {
                    // And then for each feature in the result
                    foreach (Feature resultFeature in result)
                    {
                        // Get a reference to the feature's table
                        ArcGISFeatureTable relatedTable = (ArcGISFeatureTable)resultFeature.FeatureTable;

                        // Get the display field name - this is the name of the field that is intended for display
                        string displayFieldName = relatedTable.LayerInfo.DisplayFieldName;

                        // Get the name of the feature's table
                        string tableName = relatedTable.TableName;

                        // Get the display name for the feature
                        string featureDisplayname = resultFeature.Attributes[displayFieldName].ToString();

                        // Create a formatted result string
                        string formattedResult = $"{tableName} - {featureDisplayname}";

                        // Add the result to the list
                        queryResultsForUi.Add(formattedResult);
                    }
                }

                // Create an array adapter for the layer display
                ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, queryResultsForUi);

                // Apply the adapter to show the results in the UI
                _myDisplayList.Adapter = adapter;
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the listview for displaying results
            _myDisplayList = new ListView(this);

            // Create a scrollviewer for the listview
            ScrollView myScrollView = new ScrollView(this);

            // Set the height so that it always appears on screen
            myScrollView.SetMinimumHeight(Resources.DisplayMetrics.HeightPixels / 3);
            myScrollView.FillViewport = true;

            // Add the listview to the scroll view
            myScrollView.AddView(_myDisplayList);

            // Add the scroll view to the layout
            layout.AddView(myScrollView);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}