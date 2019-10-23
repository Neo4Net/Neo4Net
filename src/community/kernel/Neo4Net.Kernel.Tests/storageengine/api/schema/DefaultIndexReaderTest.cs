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
namespace Neo4Net.Kernel.Api.StorageEngine.schema
{
	using Test = org.junit.jupiter.api.Test;

	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class DefaultIndexReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void defaultQueryImplementationMustThrowForUnsupportedIndexOrder()
		 internal virtual void DefaultQueryImplementationMustThrowForUnsupportedIndexOrder()
		 {
			  // Given
			  IndexReader indexReader = StubIndexReader();

			  // Then
			  string expectedMessage = string.Format( "This reader only have support for index order {0}. Provided index order was {1}.", IndexOrder.NONE, IndexOrder.ASCENDING );
			  System.NotSupportedException operationException = assertThrows( typeof( System.NotSupportedException ), () => indexReader.Query(new SimpleNodeValueClient(), IndexOrder.ASCENDING, false, IndexQuery.exists(1)) );
			  assertEquals( expectedMessage, operationException.Message );
		 }

		 private static IndexReader StubIndexReader()
		 {
			  return new AbstractIndexReaderAnonymousInnerClass();
		 }

		 private class AbstractIndexReaderAnonymousInnerClass : AbstractIndexReader
		 {
			 public AbstractIndexReaderAnonymousInnerClass() : base(null)
			 {
			 }

			 public override long countIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
			 {
				  return 0;
			 }

			 public override IndexSampler createSampler()
			 {
				  return null;
			 }

			 public override PrimitiveLongResourceIterator query( params IndexQuery[] predicates )
			 {
				  return null;
			 }

			 public override bool hasFullValuePrecision( params IndexQuery[] predicates )
			 {
				  return false;
			 }

			 public override void distinctValues( IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
			 {
			 }

			 public override void close()
			 {
			 }
		 }
	}

}