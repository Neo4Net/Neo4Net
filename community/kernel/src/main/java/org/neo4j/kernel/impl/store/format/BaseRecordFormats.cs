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
namespace Org.Neo4j.Kernel.impl.store.format
{

	using MetaDataRecordFormat = Org.Neo4j.Kernel.impl.store.format.standard.MetaDataRecordFormat;
	using MetaDataRecord = Org.Neo4j.Kernel.impl.store.record.MetaDataRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.contains;

	/// <summary>
	/// Base class for simpler implementation of <seealso cref="RecordFormats"/>.
	/// </summary>
	public abstract class BaseRecordFormats : RecordFormats
	{
		public abstract string Name();
		public abstract FormatFamily FormatFamily { get; }
		public abstract RecordFormat<Org.Neo4j.Kernel.impl.store.record.DynamicRecord> Dynamic();
		public abstract RecordFormat<Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord> RelationshipTypeToken();
		public abstract RecordFormat<Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord> PropertyKeyToken();
		public abstract RecordFormat<Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord> LabelToken();
		public abstract RecordFormat<Org.Neo4j.Kernel.impl.store.record.PropertyRecord> Property();
		public abstract RecordFormat<Org.Neo4j.Kernel.impl.store.record.RelationshipRecord> Relationship();
		public abstract RecordFormat<Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord> RelationshipGroup();
		public abstract RecordFormat<Org.Neo4j.Kernel.impl.store.record.NodeRecord> Node();
		 private readonly int _generation;
		 private readonly Capability[] _capabilities;
		 private readonly string _storeVersion;
		 private readonly string _introductionVersion;

		 protected internal BaseRecordFormats( string storeVersion, string introductionVersion, int generation, params Capability[] capabilities )
		 {
			  this._storeVersion = storeVersion;
			  this._generation = generation;
			  this._capabilities = capabilities;
			  this._introductionVersion = introductionVersion;
		 }

		 public override string StoreVersion()
		 {
			  return _storeVersion;
		 }

		 public override string IntroductionVersion()
		 {
			  return _introductionVersion;
		 }

		 public override RecordFormat<MetaDataRecord> MetaData()
		 {
			  return new MetaDataRecordFormat();
		 }

		 public override bool Equals( object obj )
		 {
			  if ( !( obj is RecordFormats ) )
			  {
					return false;
			  }

			  RecordFormats other = ( RecordFormats ) obj;
			  return Node().Equals(other.Node()) && Relationship().Equals(other.Relationship()) && RelationshipGroup().Equals(other.RelationshipGroup()) && Property().Equals(other.Property()) && LabelToken().Equals(other.LabelToken()) && RelationshipTypeToken().Equals(other.RelationshipTypeToken()) && PropertyKeyToken().Equals(other.PropertyKeyToken()) && Dynamic().Equals(other.Dynamic());
		 }

		 public override int GetHashCode()
		 {
			  int hashCode = 17;
			  hashCode = 31 * hashCode + Node().GetHashCode();
			  hashCode = 31 * hashCode + Relationship().GetHashCode();
			  hashCode = 31 * hashCode + RelationshipGroup().GetHashCode();
			  hashCode = 31 * hashCode + Property().GetHashCode();
			  hashCode = 31 * hashCode + LabelToken().GetHashCode();
			  hashCode = 31 * hashCode + RelationshipTypeToken().GetHashCode();
			  hashCode = 31 * hashCode + PropertyKeyToken().GetHashCode();
			  hashCode = 31 * hashCode + Dynamic().GetHashCode();
			  return hashCode;
		 }

		 public override string ToString()
		 {
			  return "RecordFormat:" + this.GetType().Name + "[" + StoreVersion() + "]";
		 }

		 public override int Generation()
		 {
			  return _generation;
		 }

		 public override Capability[] Capabilities()
		 {
			  return _capabilities;
		 }

		 public override bool HasCapability( Capability capability )
		 {
			  return contains( Capabilities(), capability );
		 }

		 public static bool HasCompatibleCapabilities( RecordFormats one, RecordFormats other, CapabilityType type )
		 {
			  ISet<Capability> myFormatCapabilities = Stream.of( one.Capabilities() ).filter(capability => capability.isType(type)).collect(toSet());
			  ISet<Capability> otherFormatCapabilities = Stream.of( other.Capabilities() ).filter(capability => capability.isType(type)).collect(toSet());

			  if ( myFormatCapabilities.SetEquals( otherFormatCapabilities ) )
			  {
					// If they have the same capabilities then of course they are compatible
					return true;
			  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  bool capabilitiesNotRemoved = otherFormatCapabilities.containsAll( myFormatCapabilities );

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
			  otherFormatCapabilities.removeAll( myFormatCapabilities );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  bool allAddedAreAdditive = otherFormatCapabilities.All( Capability::isAdditive );

			  // Even if capabilities of the two aren't the same then there's a special case where if the additional
			  // capabilities of the other format are all additive then they are also compatible because no data
			  // in the existing store needs to be migrated.
			  return capabilitiesNotRemoved && allAddedAreAdditive;
		 }

		 public override bool HasCompatibleCapabilities( RecordFormats other, CapabilityType type )
		 {
			  return HasCompatibleCapabilities( this, other, type );
		 }
	}

}