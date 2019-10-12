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
namespace Org.Neo4j.Kernel.impl.transaction.state
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using StoreHeader = Org.Neo4j.Kernel.impl.store.StoreHeader;
	using Capability = Org.Neo4j.Kernel.impl.store.format.Capability;
	using CapabilityType = Org.Neo4j.Kernel.impl.store.format.CapabilityType;
	using FormatFamily = Org.Neo4j.Kernel.impl.store.format.FormatFamily;
	using Org.Neo4j.Kernel.impl.store.format;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using StandardFormatFamily = Org.Neo4j.Kernel.impl.store.format.standard.StandardFormatFamily;
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using MetaDataRecord = Org.Neo4j.Kernel.impl.store.record.MetaDataRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

	public class PrepareTrackingRecordFormats : RecordFormats
	{
		 private readonly RecordFormats _actual;
		 private readonly ISet<NodeRecord> _nodePrepare = new HashSet<NodeRecord>();
		 private readonly ISet<RelationshipRecord> _relationshipPrepare = new HashSet<RelationshipRecord>();
		 private readonly ISet<RelationshipGroupRecord> _relationshipGroupPrepare = new HashSet<RelationshipGroupRecord>();
		 private readonly ISet<PropertyRecord> _propertyPrepare = new HashSet<PropertyRecord>();
		 private readonly ISet<DynamicRecord> _dynamicPrepare = new HashSet<DynamicRecord>();
		 private readonly ISet<PropertyKeyTokenRecord> _propertyKeyTokenPrepare = new HashSet<PropertyKeyTokenRecord>();
		 private readonly ISet<LabelTokenRecord> _labelTokenPrepare = new HashSet<LabelTokenRecord>();
		 private readonly ISet<RelationshipTypeTokenRecord> _relationshipTypeTokenPrepare = new HashSet<RelationshipTypeTokenRecord>();
		 private readonly ISet<MetaDataRecord> _metaDataPrepare = new HashSet<MetaDataRecord>();

		 public PrepareTrackingRecordFormats( RecordFormats actual )
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

		 public override PrepareTrackingRecordFormat<NodeRecord> Node()
		 {
			  return new PrepareTrackingRecordFormat<NodeRecord>( this, _actual.node(), _nodePrepare );
		 }

		 public override PrepareTrackingRecordFormat<RelationshipGroupRecord> RelationshipGroup()
		 {
			  return new PrepareTrackingRecordFormat<RelationshipGroupRecord>( this, _actual.relationshipGroup(), _relationshipGroupPrepare );
		 }

		 public override PrepareTrackingRecordFormat<RelationshipRecord> Relationship()
		 {
			  return new PrepareTrackingRecordFormat<RelationshipRecord>( this, _actual.relationship(), _relationshipPrepare );
		 }

		 public override PrepareTrackingRecordFormat<PropertyRecord> Property()
		 {
			  return new PrepareTrackingRecordFormat<PropertyRecord>( this, _actual.property(), _propertyPrepare );
		 }

		 public override PrepareTrackingRecordFormat<LabelTokenRecord> LabelToken()
		 {
			  return new PrepareTrackingRecordFormat<LabelTokenRecord>( this, _actual.labelToken(), _labelTokenPrepare );
		 }

		 public override PrepareTrackingRecordFormat<PropertyKeyTokenRecord> PropertyKeyToken()
		 {
			  return new PrepareTrackingRecordFormat<PropertyKeyTokenRecord>( this, _actual.propertyKeyToken(), _propertyKeyTokenPrepare );
		 }

		 public override PrepareTrackingRecordFormat<RelationshipTypeTokenRecord> RelationshipTypeToken()
		 {
			  return new PrepareTrackingRecordFormat<RelationshipTypeTokenRecord>( this, _actual.relationshipTypeToken(), _relationshipTypeTokenPrepare );
		 }

		 public override PrepareTrackingRecordFormat<DynamicRecord> Dynamic()
		 {
			  return new PrepareTrackingRecordFormat<DynamicRecord>( this, _actual.dynamic(), _dynamicPrepare );
		 }

		 public override PrepareTrackingRecordFormat<MetaDataRecord> MetaData()
		 {
			  return new PrepareTrackingRecordFormat<MetaDataRecord>( this, _actual.metaData(), _metaDataPrepare );
		 }

		 public override Capability[] Capabilities()
		 {
			  return _actual.capabilities();
		 }

		 public override int Generation()
		 {
			  return _actual.generation();
		 }

		 public override bool HasCapability( Capability capability )
		 {
			  return _actual.hasCapability( capability );
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
			  return _actual.hasCompatibleCapabilities( other, type );
		 }

		 public override string Name()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return this.GetType().FullName;
		 }

		 public class PrepareTrackingRecordFormat<RECORD> : RecordFormat<RECORD> where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			 private readonly PrepareTrackingRecordFormats _outerInstance;

			  internal readonly RecordFormat<RECORD> Actual;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ISet<RECORD> PrepareConflict;

			  internal PrepareTrackingRecordFormat( PrepareTrackingRecordFormats outerInstance, RecordFormat<RECORD> actual, ISet<RECORD> prepare )
			  {
				  this._outerInstance = outerInstance;
					this.Actual = actual;
					this.PrepareConflict = prepare;
			  }

			  public override RECORD NewRecord()
			  {
					return Actual.newRecord();
			  }

			  public override int GetRecordSize( StoreHeader storeHeader )
			  {
					return Actual.getRecordSize( storeHeader );
			  }

			  public virtual int RecordHeaderSize
			  {
				  get
				  {
						return Actual.RecordHeaderSize;
				  }
			  }

			  public override bool IsInUse( PageCursor cursor )
			  {
					return Actual.isInUse( cursor );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void read(RECORD record, org.neo4j.io.pagecache.PageCursor cursor, org.neo4j.kernel.impl.store.record.RecordLoad mode, int recordSize) throws java.io.IOException
			  public override void Read( RECORD record, PageCursor cursor, RecordLoad mode, int recordSize )
			  {
					Actual.read( record, cursor, mode, recordSize );
			  }

			  public override void Prepare( RECORD record, int recordSize, IdSequence idSequence )
			  {
					PrepareConflict.Add( record );
					Actual.prepare( record, recordSize, idSequence );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(RECORD record, org.neo4j.io.pagecache.PageCursor cursor, int recordSize) throws java.io.IOException
			  public override void Write( RECORD record, PageCursor cursor, int recordSize )
			  {
					Actual.write( record, cursor, recordSize );
			  }

			  public override long GetNextRecordReference( RECORD record )
			  {
					return Actual.getNextRecordReference( record );
			  }

			  public virtual long MaxId
			  {
				  get
				  {
						return Actual.MaxId;
				  }
			  }

			  public override bool Equals( object otherFormat )
			  {
					return Actual.Equals( otherFormat );
			  }

			  public override int GetHashCode()
			  {
					return Actual.GetHashCode();
			  }

			  public virtual bool Prepared( RECORD record )
			  {
					return PrepareConflict.Contains( record );
			  }
		 }
	}

}