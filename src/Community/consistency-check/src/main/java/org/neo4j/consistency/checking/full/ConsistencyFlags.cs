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
namespace Neo4Net.Consistency.checking.full
{
	using EqualsBuilder = org.apache.commons.lang3.builder.EqualsBuilder;
	using HashCodeBuilder = org.apache.commons.lang3.builder.HashCodeBuilder;

	using Config = Neo4Net.Kernel.configuration.Config;

	public class ConsistencyFlags
	{
		 private readonly bool _checkGraph;
		 private readonly bool _checkIndexes;
		 private readonly bool _checkIndexStructure;
		 private readonly bool _checkLabelScanStore;
		 private readonly bool _checkPropertyOwners;

		 public ConsistencyFlags( Config tuningConfiguration ) : this( tuningConfiguration.Get( ConsistencyCheckSettings.consistency_check_graph ), tuningConfiguration.Get( ConsistencyCheckSettings.consistency_check_indexes ), tuningConfiguration.Get( ConsistencyCheckSettings.consistency_check_index_structure ), tuningConfiguration.Get( ConsistencyCheckSettings.consistency_check_label_scan_store ), tuningConfiguration.Get( ConsistencyCheckSettings.consistency_check_property_owners ) )
		 {
		 }

		 public ConsistencyFlags( bool checkGraph, bool checkIndexes, bool checkIndexStructure, bool checkLabelScanStore, bool checkPropertyOwners )
		 {
			  this._checkGraph = checkGraph;
			  this._checkIndexes = checkIndexes;
			  this._checkIndexStructure = checkIndexStructure;
			  this._checkLabelScanStore = checkLabelScanStore;
			  this._checkPropertyOwners = checkPropertyOwners;
		 }

		 public virtual bool CheckGraph
		 {
			 get
			 {
				  return _checkGraph;
			 }
		 }

		 public virtual bool CheckIndexes
		 {
			 get
			 {
				  return _checkIndexes;
			 }
		 }

		 public virtual bool CheckIndexStructure
		 {
			 get
			 {
				  return _checkIndexStructure;
			 }
		 }

		 public virtual bool CheckLabelScanStore
		 {
			 get
			 {
				  return _checkLabelScanStore;
			 }
		 }

		 public virtual bool CheckPropertyOwners
		 {
			 get
			 {
				  return _checkPropertyOwners;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  return EqualsBuilder.reflectionEquals( this, o );
		 }

		 public override int GetHashCode()
		 {
			  return HashCodeBuilder.reflectionHashCode( this );
		 }
	}

}