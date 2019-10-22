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
namespace Neo4Net.GraphDb
{

	using IndexDefinition = Neo4Net.GraphDb.schema.IndexDefinition;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.RelationshipType.withName;

	public class FacadeMethod<T> : System.Action<T>
	{
		 public static readonly Label Label = Label.label( "Label" );
		 public static readonly RelationshipType Foo = withName( "foo" );
		 public static readonly RelationshipType Bar = withName( "bar" );
		 public static readonly Label Quux = label( "quux" );
		 public static readonly IndexDefinition IndexDefinition = mock( typeof( IndexDefinition ) );

		 private readonly string _methodSignature;
		 private readonly System.Action<T> _callable;

		 public FacadeMethod( string methodSignature, System.Action<T> callable )
		 {
			  this._methodSignature = methodSignature;
			  this._callable = callable;
		 }

		 public override void Accept( T t )
		 {
			  _callable.accept( t );
		 }

		 public virtual void Call( T self )
		 {
			  _callable.accept( self );
		 }

		 public override string ToString()
		 {
			  return _methodSignature;
		 }

		 public static void Consume<T>( IEnumerator<T> iterator )
		 {
			  IEnumerable<T> iterable = () => iterator;
			  Consume( iterable );
		 }

		 public static void Consume<T1>( IEnumerable<T1> iterable )
		 {
			  foreach ( object o in iterable )
			  {
					assertNotNull( o );
			  }
		 }
	}

}