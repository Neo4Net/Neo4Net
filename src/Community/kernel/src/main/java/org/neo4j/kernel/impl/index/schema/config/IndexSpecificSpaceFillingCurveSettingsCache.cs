using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Index.Schema.config
{

	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;

	/// <summary>
	/// A combination of <seealso cref="ConfiguredSpaceFillingCurveSettingsCache"/>, which contains all settings from <seealso cref="Config"/>,
	/// but also settings for a specific index. Settings for a specific index can change over time as new <seealso cref="CoordinateReferenceSystem"/>
	/// are used in this index.
	/// </summary>
	public class IndexSpecificSpaceFillingCurveSettingsCache
	{
		 private readonly ConfiguredSpaceFillingCurveSettingsCache _globalConfigCache;
		 /// <summary>
		 /// Map of settings that are specific to this index, i.e. where there is or have been at least one value of in the index.
		 /// Modifications of this map happen by a single thread at a time, due to index updates being applied using work-sync,
		 /// but there can be concurrent readers at any point.
		 /// </summary>
		 private readonly ConcurrentMap<CoordinateReferenceSystem, SpaceFillingCurveSettings> _specificIndexConfigCache = new ConcurrentDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>();

		 public IndexSpecificSpaceFillingCurveSettingsCache( ConfiguredSpaceFillingCurveSettingsCache globalConfigCache, IDictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings> specificIndexConfigCache )
		 {
			  this._globalConfigCache = globalConfigCache;
			  this._specificIndexConfigCache.putAll( specificIndexConfigCache );
		 }

		 /// <summary>
		 /// Gets <seealso cref="SpaceFillingCurve"/> for a particular coordinate reference system's crsTableId and code point.
		 /// </summary>
		 /// <param name="crsTableId"> table id of the <seealso cref="CoordinateReferenceSystem"/>. </param>
		 /// <param name="crsCodePoint"> code of the <seealso cref="CoordinateReferenceSystem"/>. </param>
		 /// <param name="assignToIndexIfNotYetAssigned"> whether or not to make a snapshot of this index-specific setting if this is the
		 /// first time it's accessed for this index. It will then show up in <seealso cref="visitIndexSpecificSettings(SettingVisitor)"/>. </param>
		 /// <returns> the <seealso cref="SpaceFillingCurve"/> for the given coordinate reference system. </returns>
		 public virtual SpaceFillingCurve ForCrs( int crsTableId, int crsCodePoint, bool assignToIndexIfNotYetAssigned )
		 {
			  CoordinateReferenceSystem crs = CoordinateReferenceSystem.get( crsTableId, crsCodePoint );
			  return ForCrs( crs, assignToIndexIfNotYetAssigned );
		 }

		 public virtual SpaceFillingCurve ForCrs( CoordinateReferenceSystem crs, bool assignToIndexIfNotYetAssigned )
		 {
			  // Index-specific
			  SpaceFillingCurveSettings specificSetting = _specificIndexConfigCache.get( crs );
			  if ( specificSetting != null )
			  {
					return specificSetting.Curve();
			  }

			  // Global config
			  SpaceFillingCurveSettings configuredSetting = _globalConfigCache.forCRS( crs );
			  if ( assignToIndexIfNotYetAssigned )
			  {
					_specificIndexConfigCache.put( crs, configuredSetting );
			  }
			  return configuredSetting.Curve();
		 }

		 /// <summary>
		 /// Mostly for checkpoints to serialize index-specific settings into the index header.
		 /// </summary>
		 public virtual void VisitIndexSpecificSettings( SettingVisitor visitor )
		 {
			  visitor.Count( _specificIndexConfigCache.size() );
			  _specificIndexConfigCache.forEach( visitor.visit );
		 }

		 public interface SettingVisitor
		 {
			  void Count( int count );

			  void Visit( CoordinateReferenceSystem crs, SpaceFillingCurveSettings settings );
		 }
	}

}