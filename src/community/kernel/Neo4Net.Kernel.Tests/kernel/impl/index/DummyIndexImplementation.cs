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
namespace Neo4Net.Kernel.impl.index
{

	using PrimitiveLongBaseIterator = Neo4Net.Collections.PrimitiveLongCollections.PrimitiveLongBaseIterator;
	using Neo4Net.GraphDb;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using ExplicitIndex = Neo4Net.Kernel.api.ExplicitIndex;
	using ExplicitIndexHits = Neo4Net.Kernel.api.ExplicitIndexHits;
	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using ExplicitIndexProviderTransaction = Neo4Net.Kernel.spi.explicitindex.ExplicitIndexProviderTransaction;
	using IndexCommandFactory = Neo4Net.Kernel.spi.explicitindex.IndexCommandFactory;
	using IndexImplementation = Neo4Net.Kernel.spi.explicitindex.IndexImplementation;

	public class DummyIndexImplementation : LifecycleAdapter, IndexImplementation
	{
		 public override IDictionary<string, string> FillInDefaults( IDictionary<string, string> config )
		 {
			  return config;
		 }

		 public override bool ConfigMatches( IDictionary<string, string> storedConfig, IDictionary<string, string> suppliedConfig )
		 {
			  return true;
		 }

		 private bool Failing( IDictionary<string, string> config )
		 {
			  return bool.Parse( config[DummyIndexExtensionFactory.KEY_FAIL_ON_MUTATE] );
		 }

		 private class EmptyHits : PrimitiveLongBaseIterator, ExplicitIndexHits
		 {
			  public override void Close()
			  { // Nothing to close
			  }

			  public override int Size()
			  {
					return 0;
			  }

			  public override float CurrentScore()
			  {
					return 0;
			  }

			  protected internal override bool FetchNext()
			  {
					return false;
			  }
		 }

		 private static readonly ExplicitIndexHits _noHits = new EmptyHits();

		 private class EmptyExplicitIndex : ExplicitIndex
		 {
			  internal readonly bool Failing;

			  internal EmptyExplicitIndex( bool failing )
			  {
					this.Failing = failing;
			  }

			  public override void Remove( long IEntity )
			  {
					Mutate();
			  }

			  public override void Remove( long IEntity, string key )
			  {
					Mutate();
			  }

			  public override void Remove( long IEntity, string key, object value )
			  {
					Mutate();
			  }

			  public override ExplicitIndexHits Query( object queryOrQueryObject, long startNode, long endNode )
			  {
					return _noHits;
			  }

			  public override ExplicitIndexHits Query( string key, object queryOrQueryObject, long startNode, long endNode )
			  {
					return _noHits;
			  }

			  public override ExplicitIndexHits Query( object queryOrQueryObject )
			  {
					return _noHits;
			  }

			  public override ExplicitIndexHits Query( string key, object queryOrQueryObject )
			  {
					return _noHits;
			  }

			  public override ExplicitIndexHits Get( string key, object value, long startNode, long endNode )
			  {
					return _noHits;
			  }

			  public override ExplicitIndexHits Get( string key, object value )
			  {
					return _noHits;
			  }

			  public override void Drop()
			  {
					Mutate();
			  }

			  public override void AddRelationship( long IEntity, string key, object value, long startNode, long endNode )
			  {
					Mutate();
			  }

			  public override void AddNode( long IEntity, string key, object value )
			  {
					Mutate();
			  }

			  public override void RemoveRelationship( long IEntity, string key, object value, long startNode, long endNode )
			  {
					Mutate();
			  }

			  public override void RemoveRelationship( long IEntity, string key, long startNode, long endNode )
			  {
					Mutate();
			  }

			  public override void RemoveRelationship( long IEntity, long startNode, long endNode )
			  {
					Mutate();
			  }

			  internal virtual void Mutate()
			  {
					if ( Failing )
					{
						 throw new System.NotSupportedException();
					}
			  }
		 }

		 public override File GetIndexImplementationDirectory( DatabaseLayout directoryLayout )
		 {
			  return directoryLayout.DatabaseDirectory();
		 }

		 public override ExplicitIndexProviderTransaction NewTransaction( IndexCommandFactory commandFactory )
		 {
			  return new ExplicitIndexProviderTransactionAnonymousInnerClass( this );
		 }

		 private class ExplicitIndexProviderTransactionAnonymousInnerClass : ExplicitIndexProviderTransaction
		 {
			 private readonly DummyIndexImplementation _outerInstance;

			 public ExplicitIndexProviderTransactionAnonymousInnerClass( DummyIndexImplementation outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public ExplicitIndex relationshipIndex( string indexName, IDictionary<string, string> configuration )
			 {
				  return new EmptyExplicitIndex( outerInstance.failing( configuration ) );
			 }

			 public ExplicitIndex nodeIndex( string indexName, IDictionary<string, string> configuration )
			 {
				  return new EmptyExplicitIndex( outerInstance.failing( configuration ) );
			 }

			 public void close()
			 {
			 }
		 }

		 private static readonly TransactionApplier _noApplier = new Neo4Net.Kernel.Impl.Api.TransactionApplier_Adapter();

		 public override TransactionApplier NewApplier( bool recovery )
		 {
			  return _noApplier;
		 }

		 public override void Force()
		 {
		 }

		 public override ResourceIterator<File> ListStoreFiles()
		 {
			  return Iterators.emptyResourceIterator();
		 }
	}

}