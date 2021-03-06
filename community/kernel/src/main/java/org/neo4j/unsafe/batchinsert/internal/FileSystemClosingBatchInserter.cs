﻿using System.Collections.Generic;

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
namespace Org.Neo4j.@unsafe.Batchinsert.@internal
{

	using Label = Org.Neo4j.Graphdb.Label;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using ConstraintCreator = Org.Neo4j.Graphdb.schema.ConstraintCreator;
	using IndexCreator = Org.Neo4j.Graphdb.schema.IndexCreator;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;

	public class FileSystemClosingBatchInserter : BatchInserter, IndexConfigStoreProvider
	{
		 private readonly BatchInserter @delegate;
		 private readonly IndexConfigStoreProvider _configStoreProvider;
		 private readonly FileSystemAbstraction _fileSystem;

		 public FileSystemClosingBatchInserter( BatchInserter @delegate, IndexConfigStoreProvider configStoreProvider, FileSystemAbstraction fileSystem )
		 {
			  this.@delegate = @delegate;
			  this._configStoreProvider = configStoreProvider;
			  this._fileSystem = fileSystem;
		 }

		 public override long CreateNode( IDictionary<string, object> properties, params Label[] labels )
		 {
			  return @delegate.CreateNode( properties, labels );
		 }

		 public override void CreateNode( long id, IDictionary<string, object> properties, params Label[] labels )
		 {
			  @delegate.CreateNode( id, properties, labels );
		 }

		 public override bool NodeExists( long nodeId )
		 {
			  return @delegate.NodeExists( nodeId );
		 }

		 public override void SetNodeProperties( long node, IDictionary<string, object> properties )
		 {
			  @delegate.SetNodeProperties( node, properties );
		 }

		 public override bool NodeHasProperty( long node, string propertyName )
		 {
			  return @delegate.NodeHasProperty( node, propertyName );
		 }

		 public override void SetNodeLabels( long node, params Label[] labels )
		 {
			  @delegate.SetNodeLabels( node, labels );
		 }

		 public override IEnumerable<Label> GetNodeLabels( long node )
		 {
			  return @delegate.GetNodeLabels( node );
		 }

		 public override bool NodeHasLabel( long node, Label label )
		 {
			  return @delegate.NodeHasLabel( node, label );
		 }

		 public override bool RelationshipHasProperty( long relationship, string propertyName )
		 {
			  return @delegate.RelationshipHasProperty( relationship, propertyName );
		 }

		 public override void SetNodeProperty( long node, string propertyName, object propertyValue )
		 {
			  @delegate.SetNodeProperty( node, propertyName, propertyValue );
		 }

		 public override void SetRelationshipProperty( long relationship, string propertyName, object propertyValue )
		 {
			  @delegate.SetRelationshipProperty( relationship, propertyName, propertyValue );
		 }

		 public override IDictionary<string, object> GetNodeProperties( long nodeId )
		 {
			  return @delegate.GetNodeProperties( nodeId );
		 }

		 public override IEnumerable<long> GetRelationshipIds( long nodeId )
		 {
			  return @delegate.GetRelationshipIds( nodeId );
		 }

		 public override IEnumerable<BatchRelationship> GetRelationships( long nodeId )
		 {
			  return @delegate.GetRelationships( nodeId );
		 }

		 public override long CreateRelationship( long node1, long node2, RelationshipType type, IDictionary<string, object> properties )
		 {
			  return @delegate.CreateRelationship( node1, node2, type, properties );
		 }

		 public override BatchRelationship GetRelationshipById( long relId )
		 {
			  return @delegate.GetRelationshipById( relId );
		 }

		 public override void SetRelationshipProperties( long rel, IDictionary<string, object> properties )
		 {
			  @delegate.SetRelationshipProperties( rel, properties );
		 }

		 public override IDictionary<string, object> GetRelationshipProperties( long relId )
		 {
			  return @delegate.GetRelationshipProperties( relId );
		 }

		 public override void RemoveNodeProperty( long node, string property )
		 {
			  @delegate.RemoveNodeProperty( node, property );
		 }

		 public override void RemoveRelationshipProperty( long relationship, string property )
		 {
			  @delegate.RemoveRelationshipProperty( relationship, property );
		 }

		 public override IndexCreator CreateDeferredSchemaIndex( Label label )
		 {
			  return @delegate.CreateDeferredSchemaIndex( label );
		 }

		 public override ConstraintCreator CreateDeferredConstraint( Label label )
		 {
			  return @delegate.CreateDeferredConstraint( label );
		 }

		 public override void Shutdown()
		 {
			  @delegate.Shutdown();
			  CloseFileSystem();
		 }

		 public virtual string StoreDir
		 {
			 get
			 {
				  return @delegate.StoreDir;
			 }
		 }

		 private void CloseFileSystem()
		 {
			  try
			  {
					_fileSystem.Dispose();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public virtual IndexConfigStore IndexStore
		 {
			 get
			 {
				  return _configStoreProvider.IndexStore;
			 }
		 }
	}

}