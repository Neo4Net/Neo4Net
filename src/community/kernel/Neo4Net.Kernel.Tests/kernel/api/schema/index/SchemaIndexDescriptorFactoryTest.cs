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
namespace Neo4Net.Kernel.api.schema.index
{
	using Test = org.junit.Test;

	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaTestUtil.assertEquality;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaTestUtil.simpleNameLookup;

	public class SchemaIndexDescriptorFactoryTest
	{
		 private const int LABEL_ID = 0;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateIndexDescriptors()
		 public virtual void ShouldCreateIndexDescriptors()
		 {
			  IndexDescriptor desc;

			  desc = TestIndexDescriptorFactory.ForLabel( LABEL_ID, 1 );
			  assertThat( desc.Type(), equalTo(IndexDescriptor.Type.GENERAL) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniqueIndexDescriptors()
		 public virtual void ShouldCreateUniqueIndexDescriptors()
		 {
			  IndexDescriptor desc;

			  desc = TestIndexDescriptorFactory.UniqueForLabel( LABEL_ID, 1 );
			  assertThat( desc.Type(), equalTo(IndexDescriptor.Type.UNIQUE) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateIndexDescriptorsFromSchema()
		 public virtual void ShouldCreateIndexDescriptorsFromSchema()
		 {
			  IndexDescriptor desc;

			  desc = IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( LABEL_ID, 1 ) );
			  assertThat( desc.Type(), equalTo(IndexDescriptor.Type.GENERAL) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );

			  desc = IndexDescriptorFactory.uniqueForSchema( SchemaDescriptorFactory.forLabel( LABEL_ID, 1 ) );
			  assertThat( desc.Type(), equalTo(IndexDescriptor.Type.UNIQUE) );
			  assertThat( desc.Schema(), equalTo(SchemaDescriptorFactory.forLabel(LABEL_ID, 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateEqualDescriptors()
		 public virtual void ShouldCreateEqualDescriptors()
		 {
			  IndexDescriptor desc1;
			  IndexDescriptor desc2;
			  desc1 = TestIndexDescriptorFactory.ForLabel( LABEL_ID, 1 );
			  desc2 = TestIndexDescriptorFactory.ForLabel( LABEL_ID, 1 );
			  assertEquality( desc1, desc2 );

			  desc1 = TestIndexDescriptorFactory.UniqueForLabel( LABEL_ID, 1 );
			  desc2 = TestIndexDescriptorFactory.UniqueForLabel( LABEL_ID, 1 );
			  assertEquality( desc1, desc2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceUserDescriptions()
		 public virtual void ShouldGiveNiceUserDescriptions()
		 {
			  assertThat( TestIndexDescriptorFactory.ForLabel( 1, 2 ).userDescription( simpleNameLookup ), equalTo( "Index( GENERAL, :Label1(property2) )" ) );
			  assertThat( TestIndexDescriptorFactory.UniqueForLabel( 2, 4 ).userDescription( simpleNameLookup ), equalTo( "Index( UNIQUE, :Label2(property4) )" ) );
		 }
	}

}