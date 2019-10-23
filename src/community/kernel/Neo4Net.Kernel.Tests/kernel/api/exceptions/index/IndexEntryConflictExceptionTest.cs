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
namespace Neo4Net.Kernel.Api.Exceptions.index
{
	using Test = org.junit.Test;

	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.schema.SchemaUtil;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;

	public class IndexEntryConflictExceptionTest
	{
		 public const int LABEL_ID = 1;
		 public static readonly Value Value = Values.of( "hi" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeEntryConflicts()
		 public virtual void ShouldMakeEntryConflicts()
		 {
			  LabelSchemaDescriptor schema = SchemaDescriptorFactory.forLabel( LABEL_ID, 2 );
			  IndexEntryConflictException e = new IndexEntryConflictException( 0L, 1L, Value );

			  assertThat( e.EvidenceMessage( SchemaUtil.idTokenNameLookup, schema ), equalTo( "Both Node(0) and Node(1) have the label `label[1]` and property `property[2]` = 'hi'" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeEntryConflictsForOneNode()
		 public virtual void ShouldMakeEntryConflictsForOneNode()
		 {
			  LabelSchemaDescriptor schema = SchemaDescriptorFactory.forLabel( LABEL_ID, 2 );
			  IndexEntryConflictException e = new IndexEntryConflictException( 0L, StatementConstants.NO_SUCH_NODE, Value );

			  assertThat( e.EvidenceMessage( SchemaUtil.idTokenNameLookup, schema ), equalTo( "Node(0) already exists with label `label[1]` and property `property[2]` = 'hi'" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeCompositeEntryConflicts()
		 public virtual void ShouldMakeCompositeEntryConflicts()
		 {
			  LabelSchemaDescriptor schema = SchemaDescriptorFactory.forLabel( LABEL_ID, 2, 3, 4 );
			  ValueTuple values = ValueTuple.of( true, "hi", new long[]{ 6L, 4L } );
			  IndexEntryConflictException e = new IndexEntryConflictException( 0L, 1L, values );

			  assertThat( e.EvidenceMessage( SchemaUtil.idTokenNameLookup, schema ), equalTo( "Both Node(0) and Node(1) have the label `label[1]` " + "and properties `property[2]` = true, `property[3]` = 'hi', `property[4]` = [6, 4]" ) );
		 }
	}

}