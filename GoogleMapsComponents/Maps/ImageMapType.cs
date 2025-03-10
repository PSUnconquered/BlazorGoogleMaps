﻿using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleMapsComponents.Maps
{
    public class ImageMapType : IDisposable
    {
        private readonly JsObjectRef _jsObjectRef;

        public readonly Dictionary<string, List<MapEventListener>> EventListeners;

        public Guid Guid => _jsObjectRef.Guid;
        public string Name { get; private set; }

        public async static Task<ImageMapType> CreateAsync(IJSRuntime jsRuntime, string baseUrlFormat, int minZoom, int maxZoom, string name, float opacity)
        {
            var realUrl = baseUrlFormat.Replace("{z}", "' + zoom + '").Replace("{x}", "' + coord.x + '").Replace("{y}", "' + coord.y + '");
            string initOpts = @"{
                'getTileUrl': (coord, zoom) => {
                            return '" + realUrl + @"'
                        },
                'tileSize': new google.maps.Size(256, 256),
                'maxZoom': " + maxZoom.ToString() + @",
                'minZoom': " + minZoom.ToString() + @",
                'opacity': " + opacity.ToString() + @",
                'name': '" + name + @"'
            }";

            //string initOpts = @"{
            //    'getTileUrl': (coord, zoom) => {
            //                return '" + baseUrl + @"' + zoom + '/' + coord.x + '/' + coord.y;
            //            },
            //    'tileSize': new google.maps.Size(256, 256),
            //    'maxZoom': " + maxZoom.ToString() + @",
            //    'minZoom': " + minZoom.ToString() + @",
            //    'opacity': " + opacity.ToString() + @",
            //    'name': '" + name + @"'
            //}";

            var jsObjectRef = await JsObjectRef.CreateAsync(jsRuntime, "google.maps.ImageMapType", initOpts);
            var to = new ImageMapType(jsObjectRef)
            {
                Name = name
            };
            return to;
        }
        public async static Task<ImageMapType> CreateAsync(IJSRuntime jsRuntime, string baseUrlFormat, string[] subDomains, int minZoom, int maxZoom, string name, float opacity)
        {
            // check if any subdomains were provided
            if (subDomains == null || subDomains.Length == 0)
            {
                return await CreateAsync(jsRuntime, baseUrlFormat, minZoom, maxZoom, name, opacity);
            }

            var realUrl = baseUrlFormat.Replace("{z}", "' + zoom + '").Replace("{x}", "' + coord.x + '").Replace("{y}", "' + coord.y + '");
            string initOpts = @"{
                'getTileUrl': (coord, zoom) => {
                            var subDomains = ['" + String.Join("','", subDomains) + @"'];
                            var ndx = coord.y % " + subDomains.Length + @";
                            var tUrl = '" + realUrl + @"'
                            return tUrl.replace('{domain}', subDomains[ndx]);
                        },
                'tileSize': new google.maps.Size(256, 256),
                'maxZoom': " + maxZoom.ToString() + @",
                'minZoom': " + minZoom.ToString() + @",
                'opacity': " + opacity.ToString() + @",
                'name': '" + name + @"'
            }";

            var jsObjectRef = await JsObjectRef.CreateAsync(jsRuntime, "google.maps.ImageMapType", initOpts);
            var to = new ImageMapType(jsObjectRef)
            {
                Name = name
            };
            return to;
        }
        internal ImageMapType(JsObjectRef jsObjectRef)
        {
            _jsObjectRef = jsObjectRef;
            EventListeners = new Dictionary<string, List<MapEventListener>>();
        }

        public async Task<MapEventListener> AddListener(string eventName, Action handler)
        {
            JsObjectRef listenerRef = await _jsObjectRef.InvokeWithReturnedObjectRefAsync("addListener", eventName, handler);
            MapEventListener eventListener = new MapEventListener(listenerRef);

            if (!EventListeners.ContainsKey(eventName))
            {
                EventListeners.Add(eventName, new List<MapEventListener>());
            }
            EventListeners[eventName].Add(eventListener);

            return eventListener;
        }

        /// <summary>
        /// The opacity of the overlay, expressed as a number between 0 and 1. Optional. Defaults to 1.
        /// </summary>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public async Task SetOpacity(double opacity)
        {
            if (opacity > 1)
            {
                return;
            }

            if (opacity < 0)
            {
                return;
            }

            await _jsObjectRef.InvokeAsync("setOpacity", opacity);
        }

        /// <summary>
        /// The opacity of the overlay, expressed as a number between 0 and 1. Optional. Defaults to 1.
        /// </summary>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public async Task SetOpacity(decimal opacity)
        {
            await SetOpacity(Convert.ToDouble(opacity));
        }


        public void Dispose()
        {
            _jsObjectRef?.Dispose();
        }
    }
}
