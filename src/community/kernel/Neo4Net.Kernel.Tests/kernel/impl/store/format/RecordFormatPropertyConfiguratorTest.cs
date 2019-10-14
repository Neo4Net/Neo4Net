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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.impl.store.format.standard;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using StandardFormatFamily = Neo4Net.Kernel.impl.store.format.standard.StandardFormatFamily;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using MetaDataRecord = Neo4Net.Kernel.Impl.Store.Records.MetaDataRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.DEFAULT_BLOCK_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.DEFAULT_LABEL_BLOCK_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.MINIMAL_BLOCK_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.array_block_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.label_block_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.string_block_size;

	public class RecordFormatPropertyConfiguratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void keepUserDefinedFormatConfig()
		 public virtual void KeepUserDefinedFormatConfig()
		 {
			  Config config = Config.defaults( string_block_size, "36" );
			  RecordFormats recordFormats = Standard.LATEST_RECORD_FORMATS;
			  ( new RecordFormatPropertyConfigurator( recordFormats, config ) ).Configure();
			  assertEquals( "Should keep used specified value", 36, config.Get( string_block_size ).intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void overrideDefaultValuesForCurrentFormat()
		 public virtual void OverrideDefaultValuesForCurrentFormat()
		 {
			  Config config = Config.defaults();
			  int testHeaderSize = 17;
			  ResizableRecordFormats recordFormats = new ResizableRecordFormats( this, testHeaderSize );

			  ( new RecordFormatPropertyConfigurator( recordFormats, config ) ).Configure();

			  assertEquals( DEFAULT_BLOCK_SIZE - testHeaderSize, config.Get( string_block_size ).intValue() );
			  assertEquals( DEFAULT_BLOCK_SIZE - testHeaderSize, config.Get( array_block_size ).intValue() );
			  assertEquals( DEFAULT_LABEL_BLOCK_SIZE - testHeaderSize, config.Get( label_block_size ).intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkForMinimumBlockSize()
		 public virtual void CheckForMinimumBlockSize()
		 {
			  Config config = Config.defaults();
			  int testHeaderSize = 60;
			  ResizableRecordFormats recordFormats = new ResizableRecordFormats( this, testHeaderSize );

			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  ExpectedException.expectMessage( "Block size should be bigger then " + MINIMAL_BLOCK_SIZE );

			  ( new RecordFormatPropertyConfigurator( recordFormats, config ) ).Configure();
		 }

		 private class ResizableRecordFormats : RecordFormats
		 {
			 private readonly RecordFormatPropertyConfiguratorTest _outerInstance;


			  internal int DynamicRecordHeaderSize;

			  internal ResizableRecordFormats( RecordFormatPropertyConfiguratorTest outerInstance, int dynamicRecordHeaderSize )
			  {
				  this._outerInstance = outerInstance;
					this.DynamicRecordHeaderSize = dynamicRecordHeaderSize;
			  }

			  public override string StoreVersion()
			  {
					return null;
			  }

			  public override string IntroductionVersion()
			  {
					return null;
			  }

			  public override int Generation()
			  {
					return 0;
			  }

			  public override RecordFormat<NodeRecord> Node()
			  {
					return null;
			  }

			  public override RecordFormat<RelationshipGroupRecord> RelationshipGroup()
			  {
					return null;
			  }

			  public override RecordFormat<RelationshipRecord> Relationship()
			  {
					return null;
			  }

			  public override RecordFormat<PropertyRecord> Property()
			  {
					return null;
			  }

			  public override RecordFormat<LabelTokenRecord> LabelToken()
			  {
					return null;
			  }

			  public override RecordFormat<PropertyKeyTokenRecord> PropertyKeyToken()
			  {
					return null;
			  }

			  public override RecordFormat<RelationshipTypeTokenRecord> RelationshipTypeToken()
			  {
					return null;
			  }

			  public override RecordFormat<DynamicRecord> Dynamic()
			  {
					return new ResizableRecordFormat( _outerInstance, DynamicRecordHeaderSize );
			  }

			  public override RecordFormat<MetaDataRecord> MetaData()
			  {
					return null;
			  }

			  public override Capability[] Capabilities()
			  {
					return new Capability[0];
			  }

			  public override bool HasCapability( Capability capability )
			  {
					return false;
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
					return false;
			  }

			  public override string Name()
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					return this.GetType().FullName;
			  }
		 }

		 private class ResizableRecordFormat : NoRecordFormat<DynamicRecord>
		 {
			 private readonly RecordFormatPropertyConfiguratorTest _outerInstance;

			  internal int HeaderSize;

			  internal ResizableRecordFormat( RecordFormatPropertyConfiguratorTest outerInstance, int headerSize )
			  {
				  this._outerInstance = outerInstance;
					this.HeaderSize = headerSize;
			  }

			  public override int RecordHeaderSize
			  {
				  get
				  {
						return HeaderSize;
				  }
			  }
		 }
	}

}