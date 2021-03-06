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
namespace Org.Neo4j.Kernel.impl.coreapi.schema
{

	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using Label = Org.Neo4j.Graphdb.Label;
	using IndexCreator = Org.Neo4j.Graphdb.schema.IndexCreator;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;

	public class IndexCreatorImpl : IndexCreator
	{
		 private readonly ICollection<string> _propertyKeys;
		 private readonly Label _label;
		 private readonly InternalSchemaActions _actions;
		 private readonly Optional<string> _indexName;

		 public IndexCreatorImpl( InternalSchemaActions actions, Label label ) : this( actions, label, null, new List<string>() )
		 {
		 }

		 private IndexCreatorImpl( InternalSchemaActions actions, Label label, Optional<string> indexName, ICollection<string> propertyKeys )
		 {
			  this._actions = actions;
			  this._label = label;
			  this._indexName = indexName;
			  this._propertyKeys = propertyKeys;

			  AssertInUnterminatedTransaction();
		 }

		 public override IndexCreator On( string propertyKey )
		 {
			  AssertInUnterminatedTransaction();
			  return new IndexCreatorImpl( _actions, _label, _indexName, CopyAndAdd( _propertyKeys, propertyKey ) );
		 }

		 public override IndexCreator WithName( string indexName )
		 {
			  AssertInUnterminatedTransaction();
			  return new IndexCreatorImpl( _actions, _label, Optional.ofNullable( indexName ), _propertyKeys );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.schema.IndexDefinition create() throws org.neo4j.graphdb.ConstraintViolationException
		 public override IndexDefinition Create()
		 {
			  AssertInUnterminatedTransaction();

			  if ( _propertyKeys.Count == 0 )
			  {
					throw new ConstraintViolationException( "An index needs at least one property key to index" );
			  }

			  return _actions.createIndexDefinition( _label, _indexName, _propertyKeys.toArray( new string[0] ) );
		 }

		 private void AssertInUnterminatedTransaction()
		 {
			  _actions.assertInOpenTransaction();
		 }

		 private ICollection<string> CopyAndAdd( ICollection<string> propertyKeys, string propertyKey )
		 {
			  ICollection<string> ret = new List<string>( propertyKeys );
			  ret.Add( propertyKey );
			  return ret;
		 }
	}

}