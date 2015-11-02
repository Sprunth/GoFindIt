
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
		LatLng startingLocation = null;
		LatLng goalLocation = null;

		/// <summary>
		/// Keeps track of whether the real app has started
		/// </summary>
		bool ready = false;

		Random r = new Random();

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
				locMgr.RequestLocationUpdates (provider, 3000, 1, this);
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

			if (lastKnownLocation != null && !ready)
				ReadyForLocationMap ();

			float[] distance = new float[3];
			Location.DistanceBetween (goalLocation.Latitude, goalLocation.Longitude, location.Latitude, location.Longitude, distance);

			if (distance [0] < 2)
			{
				Log.Debug ("goal", "goal reached");

				GenerateNewGoalLocation ();

			}

			ZoomToCurrentLocation ();

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
			if (lastKnownLocation != null && !ready)
				ReadyForLocationMap ();
		}

		private void ReadyForLocationMap()
		{
			ready = true;

			ZoomToCurrentLocation ();
		

			startingLocation = new LatLng (lastKnownLocation.Latitude, lastKnownLocation.Longitude);

			// current position marker
			var currentPosMkrOpts = new MarkerOptions();
			currentPosMkrOpts.SetPosition (new LatLng (lastKnownLocation.Latitude, lastKnownLocation.Longitude));
			currentPosMkrOpts.SetTitle ("Current Position");
			currentPosMkrOpts.SetIcon (BitmapDescriptorFactory.DefaultMarker (BitmapDescriptorFactory.HueCyan));
			map.AddMarker (currentPosMkrOpts);


			// next goal marker
			GenerateNewGoalLocation();
		}

		private void GenerateNewGoalLocation()
		{
			// next goal marker
			var mkrOptions = new MarkerOptions ();
			var latlngOffset = 0.0006;
			goalLocation = new LatLng (lastKnownLocation.Latitude - latlngOffset + r.NextDouble () * 2 * latlngOffset, 
				lastKnownLocation.Longitude - latlngOffset + r.NextDouble () * 2 * latlngOffset);
			mkrOptions.SetPosition (goalLocation);
			mkrOptions.SetTitle ("Next Goal");
			map.AddMarker (mkrOptions);
		}

		private void ZoomToCurrentLocation()
		{
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
			
	}
}

