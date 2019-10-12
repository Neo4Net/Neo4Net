using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.ha
{

	using TestHighlyAvailableGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using Args = Org.Neo4j.Helpers.Args;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;

	public class StartInstanceInAnotherJvm
	{
		 private StartInstanceInAnotherJvm()
		 {
		 }

		 public static void Main( string[] args )
		 {
			  File dir = new File( args[0] );
			  GraphDatabaseAPI newSlave = ( GraphDatabaseAPI ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(dir).setConfig(Args.parse(args).asMap()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static Process start(String dir, java.util.Map<String, String> config) throws Exception
		 public static Process Start( string dir, IDictionary<string, string> config )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  IList<string> args = new List<string>( Arrays.asList( "java", "-cp", System.getProperty( "java.class.path" ), typeof( StartInstanceInAnotherJvm ).FullName, dir ) );
			  foreach ( KeyValuePair<string, string> property in config.SetOfKeyValuePairs() )
			  {
					args.Add( "-" + property.Key + "=" + property.Value );
			  }
			  return Runtime.Runtime.exec( args.ToArray() );
		 }
	}

}