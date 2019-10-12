using System;
using System.Collections.Generic;

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
namespace Org.Dummy.Web.Service
{

	using Configuration = org.apache.commons.configuration.Configuration;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Neo4Net.Server.plugins;
	using PluginLifecycle = Neo4Net.Server.plugins.PluginLifecycle;

	public class DummyPluginInitializer : PluginLifecycle
	{
		 public DummyPluginInitializer()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.Collection<org.neo4j.server.plugins.Injectable<?>> start(org.neo4j.graphdb.GraphDatabaseService graphDatabaseService, org.apache.commons.configuration.Configuration config)
		 public override ICollection<Injectable<object>> Start( GraphDatabaseService graphDatabaseService, Configuration config )
		 {
			  return Collections.singleton( new InjectableAnonymousInnerClass( this ) );
		 }

		 private class InjectableAnonymousInnerClass : Injectable<long>
		 {
			 private readonly DummyPluginInitializer _outerInstance;

			 public InjectableAnonymousInnerClass( DummyPluginInitializer outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public long? Value
			 {
				 get
				 {
					  return 42L;
   
				 }
			 }

			 public Type<long> Type
			 {
				 get
				 {
					  return typeof( Long );
				 }
			 }
		 }

		 public override void Stop()
		 {
		 }
	}

}