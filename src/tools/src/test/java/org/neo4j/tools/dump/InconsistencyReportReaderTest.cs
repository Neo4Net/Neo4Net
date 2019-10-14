/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.tools.dump
{
	using Test = org.junit.Test;


	using RecordType = Neo4Net.Consistency.RecordType;
	using InconsistencyMessageLogger = Neo4Net.Consistency.report.InconsistencyMessageLogger;
	using IndexEntry = Neo4Net.Consistency.store.synthetic.IndexEntry;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using FormattedLog = Neo4Net.Logging.FormattedLog;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;
	using ReportInconsistencies = Neo4Net.tools.dump.inconsistency.ReportInconsistencies;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.schema.SchemaUtil.idTokenNameLookup;


	public class InconsistencyReportReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadBasicEntities() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadBasicEntities()
		 {
			  // GIVEN
			  MemoryStream @out = new MemoryStream( 1_000 );
			  FormattedLog log = FormattedLog.toOutputStream( @out );
			  InconsistencyMessageLogger logger = new InconsistencyMessageLogger( log );
			  long nodeId = 5;
			  long indexNodeId = 7;
			  long nodeNotInTheIndexId = 17;
			  long indexId = 99;
			  long relationshipGroupId = 10;
			  long relationshipId = 15;
			  long propertyId = 20;
			  logger.Error( RecordType.NODE, new NodeRecord( nodeId ), "Some error", "something" );
			  logger.Error( RecordType.RELATIONSHIP, new RelationshipRecord( relationshipId ), "Some error", "something" );
			  logger.Error( RecordType.RELATIONSHIP_GROUP, new RelationshipGroupRecord( relationshipGroupId ), "Some error", "something" );
			  logger.Error( RecordType.PROPERTY, new PropertyRecord( propertyId ), "Some error", "something" );
			  logger.Error( RecordType.INDEX, new IndexEntry( IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( 1, 1 ) ).withId( indexNodeId ), idTokenNameLookup, 0 ), "Some index error", "Something wrong with index" );
			  logger.Error( RecordType.NODE, new NodeRecord( nodeNotInTheIndexId ), "Some index error", IndexDescriptorFactory.forSchema( forLabel( 1, 2 ), new IndexProviderDescriptor( "key", "version" ) ).withId( indexId ).ToString() );
			  string text = @out.ToString();

			  // WHEN
			  ReportInconsistencies inconsistencies = new ReportInconsistencies();
			  InconsistencyReportReader reader = new InconsistencyReportReader( inconsistencies );
			  reader.Read( new StreamReader( new StringReader( text ) ) );

			  // THEN
			  assertTrue( inconsistencies.ContainsNodeId( nodeId ) );
			  // assertTrue( inconsistencies.containsNodeId( indexNodeId ) );
			  assertTrue( inconsistencies.ContainsNodeId( nodeNotInTheIndexId ) );
			  assertTrue( inconsistencies.ContainsRelationshipId( relationshipId ) );
			  assertTrue( inconsistencies.ContainsRelationshipGroupId( relationshipGroupId ) );
			  assertTrue( inconsistencies.ContainsPropertyId( propertyId ) );
			  assertTrue( inconsistencies.ContainsSchemaIndexId( indexId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseRelationshipGroupInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseRelationshipGroupInconsistencies()
		 {
			  // Given
			  ReportInconsistencies inconsistencies = new ReportInconsistencies();
			  string text = "ERROR: The first outgoing relationship is not the first in its chain.\n" +
						 "\tRelationshipGroup[1337,type=1,out=2,in=-1,loop=-1,prev=-1,next=3,used=true,owner=4,secondaryUnitId=-1]\n" +
						 "ERROR: The first outgoing relationship is not the first in its chain.\n" +
						 "\tRelationshipGroup[4242,type=1,out=2,in=-1,loop=-1,prev=-1,next=3,used=true,owner=4,secondaryUnitId=-1]\n";

			  // When
			  InconsistencyReportReader reader = new InconsistencyReportReader( inconsistencies );
			  reader.Read( new StreamReader( new StringReader( text ) ) );

			  // Then
			  assertTrue( inconsistencies.ContainsRelationshipGroupId( 1337 ) );
			  assertTrue( inconsistencies.ContainsRelationshipGroupId( 4242 ) );
		 }
	}

}