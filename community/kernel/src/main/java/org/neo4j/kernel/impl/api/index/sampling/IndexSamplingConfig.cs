/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Impl.Api.index.sampling
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;

	public class IndexSamplingConfig
	{
		 private readonly int _sampleSizeLimit;
		 private readonly double _updateRatio;
		 private readonly bool _backgroundSampling;

		 public IndexSamplingConfig( Config config ) : this( config.Get( GraphDatabaseSettings.index_sample_size_limit ), config.Get( GraphDatabaseSettings.index_sampling_update_percentage ) / 100.0d, config.Get( GraphDatabaseSettings.index_background_sampling_enabled ) )
		 {
		 }

		 public IndexSamplingConfig( int sampleSizeLimit, double updateRatio, bool backgroundSampling )
		 {
			  this._sampleSizeLimit = sampleSizeLimit;
			  this._updateRatio = updateRatio;
			  this._backgroundSampling = backgroundSampling;
		 }

		 public virtual int SampleSizeLimit()
		 {
			  return _sampleSizeLimit;
		 }

		 public virtual double UpdateRatio()
		 {
			  return _updateRatio;
		 }

		 public virtual int JobLimit()
		 {
			  return 1;
		 }

		 public virtual bool BackgroundSampling()
		 {
			  return _backgroundSampling;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }

			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  IndexSamplingConfig that = ( IndexSamplingConfig ) o;

			  return _backgroundSampling == that._backgroundSampling && _sampleSizeLimit == that._sampleSizeLimit && that._updateRatio.CompareTo( _updateRatio ) == 0;
		 }

		 public override int GetHashCode()
		 {
			  int result = _sampleSizeLimit;
			  long temp = System.BitConverter.DoubleToInt64Bits( _updateRatio );
			  result = 31 * result + ( int )( temp ^ ( ( long )( ( ulong )temp >> 32 ) ) );
			  result = 31 * result + ( _backgroundSampling ? 1 : 0 );
			  return result;
		 }
	}

}