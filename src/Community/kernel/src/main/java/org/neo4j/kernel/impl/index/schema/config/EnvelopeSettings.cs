using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.Impl.Index.Schema.config
{

	using ConfigValue = Neo4Net.Configuration.ConfigValue;
	using Envelope = Neo4Net.Gis.Spatial.Index.Envelope;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

	internal class EnvelopeSettings
	{
		 private const string SPATIAL_SETTING_PREFIX = "unsupported.dbms.db.spatial.crs.";
		 private const double DEFAULT_MIN_EXTENT = -1000000;
		 private const double DEFAULT_MAX_EXTENT = 1000000;
		 private const double DEFAULT_MIN_LATITUDE = -90;
		 private const double DEFAULT_MAX_LATITUDE = 90;
		 private const double DEFAULT_MIN_LONGITUDE = -180;
		 private const double DEFAULT_MAX_LONGITUDE = 180;

		 private CoordinateReferenceSystem _crs;
		 private double[] _min;
		 private double[] _max;

		 internal EnvelopeSettings( CoordinateReferenceSystem crs )
		 {
			  this._crs = crs;
			  this._min = new double[crs.Dimension];
			  this._max = new double[crs.Dimension];
			  Arrays.fill( this._min, Double.NaN );
			  Arrays.fill( this._max, Double.NaN );
		 }

		 internal static Dictionary<CoordinateReferenceSystem, EnvelopeSettings> EnvelopeSettingsFromConfig( Config config )
		 {
			  Dictionary<CoordinateReferenceSystem, EnvelopeSettings> env = new Dictionary<CoordinateReferenceSystem, EnvelopeSettings>();
			  foreach ( KeyValuePair<string, ConfigValue> entry in config.ConfigValues.SetOfKeyValuePairs() )
			  {
					string key = entry.Key;
					string value = entry.Value.ToString();
					if ( key.StartsWith( SPATIAL_SETTING_PREFIX, StringComparison.Ordinal ) )
					{
						 string[] fields = key.Replace( SPATIAL_SETTING_PREFIX, "" ).Split( "\\.", true );
						 if ( fields.Length != 3 )
						 {
							  throw new System.ArgumentException( "Invalid spatial config settings, expected three fields after '" + SPATIAL_SETTING_PREFIX + "': " + key );
						 }
						 else
						 {
							  CoordinateReferenceSystem crs = CoordinateReferenceSystem.byName( fields[0] );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
							  EnvelopeSettings envelopeSettings = env.computeIfAbsent( crs, EnvelopeSettings::new );
							  int index = "xyz".IndexOf( fields[1].ToLower(), StringComparison.Ordinal );
							  if ( index < 0 )
							  {
									throw new System.ArgumentException( "Invalid spatial coordinate key (should be one of 'x', 'y' or 'z'): " + fields[1] );
							  }
							  if ( index >= crs.Dimension )
							  {
									throw new System.ArgumentException( "Invalid spatial coordinate key for " + crs.Dimension + "D: " + fields[1] );
							  }
							  switch ( fields[2].ToLower() )
							  {
							  case "min":
									envelopeSettings._min[index] = double.Parse( value );
									break;
							  case "max":
									envelopeSettings._max[index] = double.Parse( value );
									break;
							  default:
									throw new System.ArgumentException( "Invalid spatial coordinate range key (should be one of 'max' or 'min'): " + fields[2] );
							  }
						 }
					}
			  }
			  return env;
		 }

		 internal virtual Envelope AsEnvelope()
		 {
			  int dimension = _crs.Dimension;
			  Debug.Assert( dimension >= 2 );
			  double[] min = new double[dimension];
			  double[] max = new double[dimension];
			  int cartesianStartIndex = 0;
			  if ( _crs.Geographic )
			  {
					// Geographic CRS default to extent of the earth in degrees
					min[0] = MinOrDefault( 0, DEFAULT_MIN_LONGITUDE );
					max[0] = MaxOrDefault( 0, DEFAULT_MAX_LONGITUDE );
					min[1] = MinOrDefault( 1, DEFAULT_MIN_LATITUDE );
					max[1] = MaxOrDefault( 1, DEFAULT_MAX_LATITUDE );
					cartesianStartIndex = 2; // if geographic index has higher than 2D, then other dimensions are cartesian
			  }
			  for ( int i = cartesianStartIndex; i < dimension; i++ )
			  {
					min[i] = MinOrDefault( i, DEFAULT_MIN_EXTENT );
					max[i] = MaxOrDefault( i, DEFAULT_MAX_EXTENT );
			  }
			  return new Envelope( min, max );
		 }

		 internal virtual CoordinateReferenceSystem Crs
		 {
			 get
			 {
				  return _crs;
			 }
		 }

		 private double MinOrDefault( int i, double defVal )
		 {
			  return ValOrDefault( _min[i], defVal );
		 }

		 private double MaxOrDefault( int i, double defVal )
		 {
			  return ValOrDefault( _max[i], defVal );
		 }

		 private static double ValOrDefault( double val, double def )
		 {
			  return double.IsNaN( val ) ? def : val;
		 }
	}

}