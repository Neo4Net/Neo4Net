using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store.format
{

	using StandardFormatFamily = Neo4Net.Kernel.impl.store.format.standard.StandardFormatFamily;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using MetaDataRecord = Neo4Net.Kernel.Impl.Store.Records.MetaDataRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

	/// <summary>
	/// Wraps another <seealso cref="RecordFormats"/> and merely forces <seealso cref="AbstractBaseRecord.setSecondaryUnitId(long)"/>
	/// to be used, and in extension <seealso cref="IdSequence.nextId()"/> to be called in
	/// <seealso cref="RecordFormat.prepare(AbstractBaseRecord, int, IdSequence)"/>. All <seealso cref="RecordFormat"/> instances
	/// will also be wrapped. This is a utility to test behavior when there are secondary record units at play.
	/// </summary>
	public class ForcedSecondaryUnitRecordFormats : RecordFormats
	{
		 private readonly RecordFormats _actual;

		 public ForcedSecondaryUnitRecordFormats( RecordFormats actual )
		 {
			  this._actual = actual;
		 }

		 public override string StoreVersion()
		 {
			  return _actual.storeVersion();
		 }

		 public override string IntroductionVersion()
		 {
			  return _actual.introductionVersion();
		 }

		 public override int Generation()
		 {
			  return _actual.generation();
		 }

		 private static RecordFormat<R> WithForcedSecondaryUnit<R>( RecordFormat<R> format ) where R : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  return new ForcedSecondaryUnitRecordFormat<R>( format );
		 }

		 public override RecordFormat<NodeRecord> Node()
		 {
			  return WithForcedSecondaryUnit( _actual.node() );
		 }

		 public override RecordFormat<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return WithForcedSecondaryUnit( _actual.relationshipGroup() );
		 }

		 public override RecordFormat<RelationshipRecord> Relationship()
		 {
			  return WithForcedSecondaryUnit( _actual.relationship() );
		 }

		 public override RecordFormat<PropertyRecord> Property()
		 {
			  return WithForcedSecondaryUnit( _actual.property() );
		 }

		 public override RecordFormat<LabelTokenRecord> LabelToken()
		 {
			  return WithForcedSecondaryUnit( _actual.labelToken() );
		 }

		 public override RecordFormat<PropertyKeyTokenRecord> PropertyKeyToken()
		 {
			  return WithForcedSecondaryUnit( _actual.propertyKeyToken() );
		 }

		 public override RecordFormat<RelationshipTypeTokenRecord> RelationshipTypeToken()
		 {
			  return WithForcedSecondaryUnit( _actual.relationshipTypeToken() );
		 }

		 public override RecordFormat<DynamicRecord> Dynamic()
		 {
			  return WithForcedSecondaryUnit( _actual.dynamic() );
		 }

		 public override RecordFormat<MetaDataRecord> MetaData()
		 {
			  return WithForcedSecondaryUnit( _actual.metaData() );
		 }

		 public override Capability[] Capabilities()
		 {
			  ISet<Capability> myCapabilities = Stream.of( _actual.capabilities() ).collect(toSet());
			  myCapabilities.Add( Capability.SecondaryRecordUnits );
			  return myCapabilities.toArray( new Capability[0] );
		 }

		 public override bool HasCapability( Capability capability )
		 {
			  return capability == Capability.SecondaryRecordUnits || _actual.hasCapability( capability );
		 }

		 public virtual FormatFamily FormatFamily
		 {
			 get
			 {
				  return StandardFormatFamily.INSTANCE;
			 }
		 }

		 public override bool HasCompatibleCapabilities( RecordFormats other, CapabilityType type )
		 {
			  return BaseRecordFormats.HasCompatibleCapabilities( this, other, type );
		 }

		 public override string Name()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return this.GetType().FullName;
		 }
	}

}