using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.Storageengine.Api.txstate
{
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;


	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;

	/// <summary>
	/// A visitor for visiting the changes that have been made in a transaction.
	/// </summary>
	public interface TxStateVisitor : AutoCloseable
	{
		 void VisitCreatedNode( long id );

		 void VisitDeletedNode( long id );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visitCreatedRelationship(long id, int type, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException;
		 void VisitCreatedRelationship( long id, int type, long startNode, long endNode );

		 void VisitDeletedRelationship( long id );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visitNodePropertyChanges(long id, java.util.Iterator<org.neo4j.storageengine.api.StorageProperty> added, java.util.Iterator<org.neo4j.storageengine.api.StorageProperty> changed, org.eclipse.collections.api.IntIterable removed) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException;
		 void VisitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visitRelPropertyChanges(long id, java.util.Iterator<org.neo4j.storageengine.api.StorageProperty> added, java.util.Iterator<org.neo4j.storageengine.api.StorageProperty> changed, org.eclipse.collections.api.IntIterable removed) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException;
		 void VisitRelPropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed );

		 void VisitGraphPropertyChanges( IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visitNodeLabelChanges(long id, org.eclipse.collections.api.set.primitive.LongSet added, org.eclipse.collections.api.set.primitive.LongSet removed) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException;
		 void VisitNodeLabelChanges( long id, LongSet added, LongSet removed );

		 void VisitAddedIndex( IndexDescriptor element );

		 void VisitRemovedIndex( IndexDescriptor element );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visitAddedConstraint(org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor element) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException;
		 void VisitAddedConstraint( ConstraintDescriptor element );

		 void VisitRemovedConstraint( ConstraintDescriptor element );

		 void VisitCreatedLabelToken( long id, string name );

		 void VisitCreatedPropertyKeyToken( long id, string name );

		 void VisitCreatedRelationshipTypeToken( long id, string name );

		 void Close();

		 /// <summary>
		 /// Interface for allowing decoration of a TxStateVisitor with one or more other visitor(s).
		 /// </summary>
	}

	public static class TxStateVisitor_Fields
	{
		 public static readonly TxStateVisitor Empty = new Adapter();
		 public static readonly Decorator NoDecoration = txStateVisitor => txStateVisitor;
	}

	 public class TxStateVisitor_Adapter : TxStateVisitor
	 {
		  public override void VisitCreatedNode( long id )
		  {
		  }

		  public override void VisitDeletedNode( long id )
		  {
		  }

		  public override void VisitCreatedRelationship( long id, int type, long startNode, long endNode )
		  {
		  }

		  public override void VisitDeletedRelationship( long id )
		  {
		  }

		  public override void VisitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		  {
		  }

		  public override void VisitRelPropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		  {
		  }

		  public override void VisitGraphPropertyChanges( IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		  {
		  }

		  public override void VisitNodeLabelChanges( long id, LongSet added, LongSet removed )
		  {
		  }

		  public override void VisitAddedIndex( IndexDescriptor index )
		  {
		  }

		  public override void VisitRemovedIndex( IndexDescriptor index )
		  {
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitAddedConstraint(org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor element) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		  public override void VisitAddedConstraint( ConstraintDescriptor element )
		  {
		  }

		  public override void VisitRemovedConstraint( ConstraintDescriptor element )
		  {
		  }

		  public override void VisitCreatedLabelToken( long id, string name )
		  {
		  }

		  public override void VisitCreatedPropertyKeyToken( long id, string name )
		  {
		  }

		  public override void VisitCreatedRelationshipTypeToken( long id, string name )
		  {
		  }

		  public override void Close()
		  {
		  }
	 }

	 public class TxStateVisitor_Delegator : TxStateVisitor
	 {
		  internal readonly TxStateVisitor Actual;

		  public TxStateVisitor_Delegator( TxStateVisitor actual )
		  {
				Debug.Assert( actual != null );
				this.Actual = actual;
		  }

		  public override void VisitCreatedNode( long id )
		  {
				Actual.visitCreatedNode( id );
		  }

		  public override void VisitDeletedNode( long id )
		  {
				Actual.visitDeletedNode( id );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitCreatedRelationship(long id, int type, long startNode, long endNode) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException
		  public override void VisitCreatedRelationship( long id, int type, long startNode, long endNode )
		  {
				Actual.visitCreatedRelationship( id, type, startNode, endNode );
		  }

		  public override void VisitDeletedRelationship( long id )
		  {
				Actual.visitDeletedRelationship( id );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitNodePropertyChanges(long id, java.util.Iterator<org.neo4j.storageengine.api.StorageProperty> added, java.util.Iterator<org.neo4j.storageengine.api.StorageProperty> changed, org.eclipse.collections.api.IntIterable removed) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException
		  public override void VisitNodePropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		  {
				Actual.visitNodePropertyChanges( id, added, changed, removed );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitRelPropertyChanges(long id, java.util.Iterator<org.neo4j.storageengine.api.StorageProperty> added, java.util.Iterator<org.neo4j.storageengine.api.StorageProperty> changed, org.eclipse.collections.api.IntIterable removed) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException
		  public override void VisitRelPropertyChanges( long id, IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		  {
				Actual.visitRelPropertyChanges( id, added, changed, removed );
		  }

		  public override void VisitGraphPropertyChanges( IEnumerator<StorageProperty> added, IEnumerator<StorageProperty> changed, IntIterable removed )
		  {
				Actual.visitGraphPropertyChanges( added, changed, removed );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitNodeLabelChanges(long id, org.eclipse.collections.api.set.primitive.LongSet added, org.eclipse.collections.api.set.primitive.LongSet removed) throws org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException
		  public override void VisitNodeLabelChanges( long id, LongSet added, LongSet removed )
		  {
				Actual.visitNodeLabelChanges( id, added, removed );
		  }

		  public override void VisitAddedIndex( IndexDescriptor index )
		  {
				Actual.visitAddedIndex( index );
		  }

		  public override void VisitRemovedIndex( IndexDescriptor index )
		  {
				Actual.visitRemovedIndex( index );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void visitAddedConstraint(org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor constraint) throws org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException
		  public override void VisitAddedConstraint( ConstraintDescriptor constraint )
		  {
				Actual.visitAddedConstraint( constraint );
		  }

		  public override void VisitRemovedConstraint( ConstraintDescriptor constraint )
		  {
				Actual.visitRemovedConstraint( constraint );
		  }

		  public override void VisitCreatedLabelToken( long id, string name )
		  {
				Actual.visitCreatedLabelToken( id, name );
		  }

		  public override void VisitCreatedPropertyKeyToken( long id, string name )
		  {
				Actual.visitCreatedPropertyKeyToken( id, name );
		  }

		  public override void VisitCreatedRelationshipTypeToken( long id, string name )
		  {
				Actual.visitCreatedRelationshipTypeToken( id, name );
		  }

		  public override void Close()
		  {
				Actual.close();
		  }
	 }

	 public interface TxStateVisitor_Decorator : System.Func<TxStateVisitor, TxStateVisitor>
	 {
	 }

}