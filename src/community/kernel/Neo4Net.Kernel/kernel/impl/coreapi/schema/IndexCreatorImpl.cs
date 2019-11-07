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
namespace Neo4Net.Kernel.impl.coreapi.schema
{

	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Label = Neo4Net.GraphDb.Label;
	using IndexCreator = Neo4Net.GraphDb.Schema.IndexCreator;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;

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
//ORIGINAL LINE: public Neo4Net.GraphDb.Schema.IndexDefinition create() throws Neo4Net.graphdb.ConstraintViolationException
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