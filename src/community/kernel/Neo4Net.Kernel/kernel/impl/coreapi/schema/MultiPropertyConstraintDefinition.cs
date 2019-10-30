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
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	internal abstract class MultiPropertyConstraintDefinition : PropertyConstraintDefinition
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly string[] PropertyKeysConflict;

		 protected internal MultiPropertyConstraintDefinition( InternalSchemaActions actions, string[] propertyKeys ) : base( actions )
		 {
			  this.PropertyKeysConflict = RequireNonEmpty( propertyKeys );
		 }

		 protected internal MultiPropertyConstraintDefinition( InternalSchemaActions actions, IndexDefinition indexDefinition ) : base( actions )
		 {
			  this.PropertyKeysConflict = RequireNonEmpty( Iterables.asArray( typeof( string ), indexDefinition.PropertyKeys ) );
		 }

		 private static string[] RequireNonEmpty( string[] array )
		 {
			  requireNonNull( array );
			  if ( array.Length < 1 )
			  {
					throw new System.ArgumentException( "Property constraint must have at least one property" );
			  }
			  foreach ( string field in array )
			  {
					if ( string.ReferenceEquals( field, null ) )
					{
						 throw new System.ArgumentException( "Property constraints cannot have null property names" );
					}
			  }
			  return array;
		 }

		 public override IEnumerable<string> PropertyKeys
		 {
			 get
			 {
				  AssertInUnterminatedTransaction();
				  return asList( PropertyKeysConflict );
			 }
		 }
	}

}