using System;
using System.IO;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.tools.dbstructure
{

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using Neo4Net.Collections.Helpers;
	using DbStructureArgumentFormatter = Neo4Net.Kernel.impl.util.dbstructure.DbStructureArgumentFormatter;
	using DbStructureVisitor = Neo4Net.Kernel.impl.util.dbstructure.DbStructureVisitor;
	using GraphDbStructureGuide = Neo4Net.Kernel.impl.util.dbstructure.GraphDbStructureGuide;
	using Neo4Net.Kernel.impl.util.dbstructure;

	public class DbStructureTool
	{
		 protected internal DbStructureTool()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws java.io.IOException
		 public static void Main( string[] args )
		 {
			  ( new DbStructureTool() ).Run(args);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void run(String[] args) throws java.io.IOException
		 protected internal virtual void Run( string[] args )
		 {
			  if ( args.Length != 2 && args.Length != 3 )
			  {
					Console.Error.WriteLine( "arguments: <generated class name> [<output source root>] <database dir>" );
					Environment.Exit( 1 );
			  }

			  bool WriteToFile = args.Length == 3;
			  string generatedClassWithPackage = args[0];
			  string dbDir = WriteToFile ? args[2] : args[1];

			  Pair<string, string> parsedGenerated = ParseClassNameWithPackage( generatedClassWithPackage );
			  string generatedClassPackage = parsedGenerated.First();
			  string generatedClassName = parsedGenerated.Other();

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  string generator = format( "%s %s [<output source root>] <db-dir>", this.GetType().FullName, generatedClassWithPackage );

			  IGraphDatabaseService graph = InstantiateGraphDatabase( dbDir );
			  try
			  {
					if ( WriteToFile )
					{
						 File sourceRoot = new File( args[1] );
						 string outputPackageDir = generatedClassPackage.Replace( '.', Path.DirectorySeparatorChar );
						 string outputFileName = generatedClassName + ".java";
						 File outputDir = new File( sourceRoot, outputPackageDir );
						 File outputFile = new File( outputDir, outputFileName );
						 using ( PrintWriter writer = new PrintWriter( outputFile ) )
						 {
							  TraceDb( generator, generatedClassPackage, generatedClassName, graph, writer );
						 }
					}
					else
					{
						 TraceDb( generator, generatedClassPackage, generatedClassName, graph, System.out );
					}
			  }
			  finally
			  {
					graph.Shutdown();
			  }
		 }

		 protected internal virtual IGraphDatabaseService InstantiateGraphDatabase( string dbDir )
		 {
			  return ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabase(new File(dbDir));
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void traceDb(String generator, String generatedClazzPackage, String generatedClazzName, Neo4Net.graphdb.GraphDatabaseService graph, Appendable output) throws java.io.IOException
		 private void TraceDb( string generator, string generatedClazzPackage, string generatedClazzName, IGraphDatabaseService graph, Appendable output )
		 {
			  InvocationTracer<DbStructureVisitor> tracer = new InvocationTracer<DbStructureVisitor>( generator, generatedClazzPackage, generatedClazzName, typeof( DbStructureVisitor ), DbStructureArgumentFormatter.INSTANCE, output );

			  DbStructureVisitor visitor = tracer.NewProxy();
			  GraphDbStructureGuide guide = new GraphDbStructureGuide( graph );
			  guide.Accept( visitor );
			  tracer.Close();
		 }

		 private Pair<string, string> ParseClassNameWithPackage( string classNameWithPackage )
		 {
			  if ( classNameWithPackage.Contains( "%" ) )
			  {
					throw new System.ArgumentException( "Format character in generated class name: " + classNameWithPackage );
			  }

			  int index = classNameWithPackage.LastIndexOf( '.' );

			  if ( index < 0 )
			  {
					throw new System.ArgumentException( "Expected fully qualified class name but got: " + classNameWithPackage );
			  }

			  return Pair.of( classNameWithPackage.Substring( 0, index ), classNameWithPackage.Substring( index + 1 ) );
		 }
	}

}