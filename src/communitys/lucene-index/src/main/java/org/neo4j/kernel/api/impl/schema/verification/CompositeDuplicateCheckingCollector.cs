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
namespace Neo4Net.Kernel.Api.Impl.Schema.verification
{
	using Document = org.apache.lucene.document.Document;

	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using Value = Neo4Net.Values.Storable.Value;

	public class CompositeDuplicateCheckingCollector : DuplicateCheckingCollector
	{
		 private readonly int[] _propertyKeyIds;

		 internal CompositeDuplicateCheckingCollector( NodePropertyAccessor accessor, int[] propertyKeyIds ) : base( accessor, StatementConstants.NO_SUCH_PROPERTY_KEY )
		 {
			  this._propertyKeyIds = propertyKeyIds;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void doCollect(int doc) throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.KernelException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 protected internal override void DoCollect( int doc )
		 {
			  Document document = Reader.document( doc );
			  long nodeId = LuceneDocumentStructure.getNodeId( document );
			  Value[] values = new Value[_propertyKeyIds.Length];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					values[i] = Accessor.getNodePropertyValue( nodeId, _propertyKeyIds[i] );
			  }
			  DuplicateCheckStrategy.checkForDuplicate( values, nodeId );
		 }
	}

}