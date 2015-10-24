
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;

using Android.Util;

namespace GoFindIt
{
	[Activity (Label = "MapActivity")]			
	public class MapActivity : Activity, ILocationListener, IOnMapReadyCallback
	{
		MapFragment mapFrag;
		GoogleMap map = null;
		Location lastKnownLocation = null;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Map);

			// Create a map fragment for gmaps
			mapFrag = MapFragment.NewInstance ();
			var tx = FragmentManager.BeginTransaction ();
			tx.Add (Resource.Id.MapLayout, mapFrag);
			tx.Commit ();

			// Grab a ref to the location manager
			var locMgr = GetSystemService (Context.LocationService) as LocationManager;
			// use GPS for location rather than wifi/3G
			var provider = LocationManager.GpsProvider;

			if (locMgr.IsProviderEnabled (provider))
			{
				// grab GPS loc every 5000ms for 1+ meter changes, alert this class' ILocationListener
				locMgr.RequestLocationUpdates (provider, 5000, 1, this);
			}
			else
			{
				// no GPS
				throw new Exception ("ded");
			}

			// wait like an idiot till map is ready
			mapFrag.GetMapAsync(this);

		}

		/* ILocationListener interfaces */
		public void OnProviderEnabled (string provider)
		{
			
		}
		public void OnProviderDisabled (string provider)
		{

		}
		public void OnStatusChanged (string provider, Availability status, Bundle extras)
		{

		}
		public void OnLocationChanged (Android.Locations.Location location)
		{
			Log.Debug ("location", location.Latitude + ", " + location.Longitude);
			lastKnownLocation = location;

			var builder = CameraPosition.InvokeBuilder ();
			builder.Target (new LatLng (lastKnownLocation.Latitude, lastKnownLocation.Longitude));
			builder.Zoom (18);
			builder.Bearing (155);
			builder.Tilt (65);
			var camPos = builder.Build ();
			var camUpdate = CameraUpdateFactory.NewCameraPosition (camPos);

			if (map!=null)
				map.MoveCamera (camUpdate);
		}
		/* End ILocationListener interfaces */
			
		/// <summary>
		/// IOnMapReadyCallback interface
		/// </summary>
		/// <param name="map">Map.</param>
		public void OnMapReady(GoogleMap map)
		{
			Log.Info ("Map", "ready");
			this.map = map;
		}
	}
}

