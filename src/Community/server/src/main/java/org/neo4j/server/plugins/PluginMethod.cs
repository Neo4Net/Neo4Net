using System;

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
namespace Neo4Net.Server.plugins
{

	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using Representation = Neo4Net.Server.rest.repr.Representation;

	internal class PluginMethod : PluginPoint
	{
		 private readonly ServerPlugin _plugin;
		 private readonly System.Reflection.MethodInfo _method;
		 private readonly DataExtractor[] _extractors;
		 private readonly ResultConverter _result;

		 internal PluginMethod( string name, Type discovery, ServerPlugin plugin, ResultConverter result, System.Reflection.MethodInfo method, DataExtractor[] extractors, Description description ) : base( discovery, name, description == null ? "" : description.value() )
		 {
			  this._plugin = plugin;
			  this._result = result;
			  this._method = method;
			  this._extractors = extractors;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.rest.repr.Representation invoke(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object source, ParameterList params) throws BadPluginInvocationException, PluginInvocationFailureException, org.neo4j.server.rest.repr.BadInputException
		 public override Representation Invoke( GraphDatabaseAPI graphDb, object source, ParameterList @params )
		 {
			  object[] arguments = new object[_extractors.Length];
			  using ( Transaction tx = graphDb.BeginTx() )
			  {
					for ( int i = 0; i < arguments.Length; i++ )
					{
						 arguments[i] = _extractors[i].extract( graphDb, source, @params );
					}
			  }
			  try
			  {
					object returned = _method.invoke( _plugin, arguments );

					if ( returned == null )
					{
						 return Representation.emptyRepresentation();
					}
					return _result.convert( returned );
			  }
			  catch ( InvocationTargetException exc )
			  {
					Exception targetExc = exc.TargetException;
					foreach ( Type excType in _method.ExceptionTypes )
					{
						 if ( excType.IsInstanceOfType( targetExc ) )
						 {
							  throw new BadPluginInvocationException( targetExc );
						 }
					}
					throw new PluginInvocationFailureException( targetExc );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is IllegalAccessException )
			  {
					throw new PluginInvocationFailureException( e );
			  }
		 }

		 protected internal override void DescribeParameters( ParameterDescriptionConsumer consumer )
		 {
			  foreach ( DataExtractor extractor in _extractors )
			  {
					extractor.Describe( consumer );
			  }
		 }

	}

}