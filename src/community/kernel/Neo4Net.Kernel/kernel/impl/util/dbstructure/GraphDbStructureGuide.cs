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
namespace Neo4Net.Kernel.impl.util.dbstructure
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Helpers.Collections;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using Read = Neo4Net.Kernel.Api.Internal.Read;
	using SchemaRead = Neo4Net.Kernel.Api.Internal.SchemaRead;
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using NodeExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeExistenceConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.RelExistenceConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.loop;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.IndexReference.sortByType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.StatementConstants.ANY_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.StatementConstants.ANY_RELATIONSHIP_TYPE;

	public class GraphDbStructureGuide : Visitable<DbStructureVisitor>
	{
		 private static RelationshipType _wildcardRelType = () => "";

		 private readonly IGraphDatabaseService _db;
		 private readonly ThreadToStatementContextBridge _bridge;

		 public GraphDbStructureGuide( IGraphDatabaseService graph )
		 {
			  this._db = graph;
			  DependencyResolver dependencies = ( ( GraphDatabaseAPI ) graph ).DependencyResolver;
			  this._bridge = dependencies.ResolveDependency( typeof( ThreadToStatementContextBridge ) );
		 }

		 public override void Accept( DbStructureVisitor visitor )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					ShowStructure( _bridge.getKernelTransactionBoundToThisThread( true ), visitor );
					tx.Success();
			  }
		 }

		 private void ShowStructure( KernelTransaction ktx, DbStructureVisitor visitor )
		 {

			  try
			  {
					ShowTokens( visitor, ktx );
					ShowSchema( visitor, ktx );
					ShowStatistics( visitor, ktx );
			  }
			  catch ( KernelException e )
			  {
					throw new System.InvalidOperationException( "Kernel exception when traversing database schema structure and statistics. " + "This is not expected to happen.", e );
			  }
		 }

		 private void ShowTokens( DbStructureVisitor visitor, KernelTransaction ktx )
		 {
			  ShowLabels( ktx, visitor );
			  ShowPropertyKeys( ktx, visitor );
			  ShowRelTypes( ktx, visitor );
		 }

		 private void ShowLabels( KernelTransaction ktx, DbStructureVisitor visitor )
		 {
			  foreach ( Label label in _db.AllLabels )
			  {
					int labelId = ktx.TokenRead().nodeLabel(label.Name());
					visitor.VisitLabel( labelId, label.Name() );
			  }
		 }

		 private void ShowPropertyKeys( KernelTransaction ktx, DbStructureVisitor visitor )
		 {
			  foreach ( string propertyKeyName in _db.AllPropertyKeys )
			  {
					int propertyKeyId = ktx.TokenRead().propertyKey(propertyKeyName);
					visitor.VisitPropertyKey( propertyKeyId, propertyKeyName );
			  }
		 }

		 private void ShowRelTypes( KernelTransaction ktx, DbStructureVisitor visitor )
		 {
			  foreach ( RelationshipType relType in _db.AllRelationshipTypes )
			  {
					int relTypeId = ktx.TokenRead().relationshipType(relType.Name());
					visitor.VisitRelationshipType( relTypeId, relType.Name() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void showSchema(DbStructureVisitor visitor, org.Neo4Net.kernel.api.KernelTransaction ktx) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 private void ShowSchema( DbStructureVisitor visitor, KernelTransaction ktx )
		 {
			  TokenNameLookup nameLookup = new SilentTokenNameLookup( ktx.TokenRead() );

			  ShowIndices( visitor, ktx, nameLookup );
			  ShowUniqueConstraints( visitor, ktx, nameLookup );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void showIndices(DbStructureVisitor visitor, org.Neo4Net.kernel.api.KernelTransaction ktx, org.Neo4Net.Kernel.Api.Internal.TokenNameLookup nameLookup) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException
		 private void ShowIndices( DbStructureVisitor visitor, KernelTransaction ktx, TokenNameLookup nameLookup )
		 {
			  SchemaRead schemaRead = ktx.SchemaRead();
			  foreach ( IndexReference reference in loop( sortByType( schemaRead.IndexesGetAll() ) ) )
			  {
					string userDescription = reference.Schema().userDescription(nameLookup);
					double uniqueValuesPercentage = schemaRead.IndexUniqueValuesSelectivity( reference );
					long size = schemaRead.IndexSize( reference );
					visitor.VisitIndex( ( IndexDescriptor ) reference, userDescription, uniqueValuesPercentage, size );
			  }
		 }

		 private void ShowUniqueConstraints( DbStructureVisitor visitor, KernelTransaction ktx, TokenNameLookup nameLookup )
		 {
			  IEnumerator<ConstraintDescriptor> constraints = ktx.SchemaRead().constraintsGetAll();
			  while ( constraints.MoveNext() )
			  {
					ConstraintDescriptor constraint = constraints.Current;
					string userDescription = constraint.PrettyPrint( nameLookup );

					if ( constraint is UniquenessConstraintDescriptor )
					{
						 visitor.VisitUniqueConstraint( ( UniquenessConstraintDescriptor ) constraint, userDescription );
					}
					else if ( constraint is NodeExistenceConstraintDescriptor )
					{
						 NodeExistenceConstraintDescriptor existenceConstraint = ( NodeExistenceConstraintDescriptor ) constraint;
						 visitor.VisitNodePropertyExistenceConstraint( existenceConstraint, userDescription );
					}
					else if ( constraint is RelExistenceConstraintDescriptor )
					{
						 RelExistenceConstraintDescriptor existenceConstraint = ( RelExistenceConstraintDescriptor ) constraint;
						 visitor.VisitRelationshipPropertyExistenceConstraint( existenceConstraint, userDescription );
					}
					else if ( constraint is NodeKeyConstraintDescriptor )
					{
						 NodeKeyConstraintDescriptor nodeKeyConstraint = ( NodeKeyConstraintDescriptor ) constraint;
						 visitor.VisitNodeKeyConstraint( nodeKeyConstraint, userDescription );
					}
					else
					{
						 throw new System.ArgumentException( "Unknown constraint type: " + constraint.GetType() + ", " + "constraint: " + constraint );
					}
			  }
		 }

		 private void ShowStatistics( DbStructureVisitor visitor, KernelTransaction ktx )
		 {
			  ShowNodeCounts( ktx, visitor );
			  ShowRelCounts( ktx, visitor );
		 }

		 private void ShowNodeCounts( KernelTransaction ktx, DbStructureVisitor visitor )
		 {
			  Read read = ktx.DataRead();
			  visitor.VisitAllNodesCount( read.CountsForNode( ANY_LABEL ) );
			  foreach ( Label label in _db.AllLabels )
			  {
					int labelId = ktx.TokenRead().nodeLabel(label.Name());
					visitor.VisitNodeCount( labelId, label.Name(), read.CountsForNode(labelId) );
			  }
		 }
		 private void ShowRelCounts( KernelTransaction ktx, DbStructureVisitor visitor )
		 {
			  // all wildcards
			  NoSide( ktx, visitor, _wildcardRelType, ANY_RELATIONSHIP_TYPE );

			  TokenRead tokenRead = ktx.TokenRead();
			  // one label only
			  foreach ( Label label in _db.AllLabels )
			  {
					int labelId = tokenRead.NodeLabel( label.Name() );

					LeftSide( ktx, visitor, label, labelId, _wildcardRelType, ANY_RELATIONSHIP_TYPE );
					RightSide( ktx, visitor, label, labelId, _wildcardRelType, ANY_RELATIONSHIP_TYPE );
			  }

			  // fixed rel type
			  foreach ( RelationshipType relType in _db.AllRelationshipTypes )
			  {
					int relTypeId = tokenRead.RelationshipType( relType.Name() );
					NoSide( ktx, visitor, relType, relTypeId );

					foreach ( Label label in _db.AllLabels )
					{
						 int labelId = tokenRead.NodeLabel( label.Name() );

						 // wildcard on right
						 LeftSide( ktx, visitor, label, labelId, relType, relTypeId );

						 // wildcard on left
						 RightSide( ktx, visitor, label, labelId, relType, relTypeId );
					}
			  }
		 }

		 private void NoSide( KernelTransaction ktx, DbStructureVisitor visitor, RelationshipType relType, int relTypeId )
		 {
			  string userDescription = format( "MATCH ()-[%s]->() RETURN count(*)", Colon( relType.Name() ) );
			  long amount = ktx.DataRead().countsForRelationship(ANY_LABEL, relTypeId, ANY_LABEL);

			  visitor.VisitRelCount( ANY_LABEL, relTypeId, ANY_LABEL, userDescription, amount );
		 }

		 private void LeftSide( KernelTransaction ktx, DbStructureVisitor visitor, Label label, int labelId, RelationshipType relType, int relTypeId )
		 {
			  string userDescription = format( "MATCH (%s)-[%s]->() RETURN count(*)", Colon( label.Name() ), Colon(relType.Name()) );
			  long amount = ktx.DataRead().countsForRelationship(labelId, relTypeId, ANY_LABEL);

			  visitor.VisitRelCount( labelId, relTypeId, ANY_LABEL, userDescription, amount );
		 }

		 private void RightSide( KernelTransaction ktx, DbStructureVisitor visitor, Label label, int labelId, RelationshipType relType, int relTypeId )
		 {
			  string userDescription = format( "MATCH ()-[%s]->(%s) RETURN count(*)", Colon( relType.Name() ), Colon(label.Name()) );
			  long amount = ktx.DataRead().countsForRelationship(ANY_LABEL, relTypeId, labelId);

			  visitor.VisitRelCount( ANY_LABEL, relTypeId, labelId, userDescription, amount );
		 }

		 private string Colon( string name )
		 {
			  return name.Length == 0 ? name : ( ":" + name );
		 }
	}

}