using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.tools.org.neo4j.index
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;


	using Neo4Net.Index.@internal.gbptree;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using StandalonePageCacheFactory = Neo4Net.Io.pagecache.impl.muninn.StandalonePageCacheFactory;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Command = Neo4Net.tools.console.input.Command;
	using ConsoleInput = Neo4Net.tools.console.input.ConsoleInput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_WRITER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tools.console.input.ConsoleUtil.staticPrompt;

	public class GBPTreePlayground
	{
		 private readonly File _indexFile;
		 private GBPTree<MutableLong, MutableLong> _tree;

		 private int _pageSize = 256;
		 private PageCache _pageCache;
		 private SimpleLongLayout _layout;
		 private readonly MutableBoolean _autoPrint = new MutableBoolean( true );

		 private GBPTreePlayground( FileSystemAbstraction fs, File indexFile )
		 {
			  this._indexFile = indexFile;
			  this._layout = SimpleLongLayout.LongLayout().build();
			  this._pageCache = StandalonePageCacheFactory.createPageCache( fs, createInitializedScheduler() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setupIndex() throws java.io.IOException
		 private void SetupIndex()
		 {
			  RecoveryCleanupWorkCollector recoveryCleanupWorkCollector = RecoveryCleanupWorkCollector.Immediate();
			  bool readOnly = true;
			 // tree = new GBPTree<>( pageCache, indexFile, layout, pageSize, NO_MONITOR, NO_HEADER_READER, NO_HEADER_WRITER,
			 //         RecoveryCleanupWorkCollector.immediate() );
			  _tree = new GBPTree<MutableLong, MutableLong>( _pageCache, _indexFile, _layout, _pageSize, NO_MONITOR, NO_HEADER_READER, NO_HEADER_WRITER, recoveryCleanupWorkCollector, readOnly );

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void run() throws InterruptedException, java.io.IOException
		 private void Run()
		 {
			  Console.WriteLine( "Working on: " + _indexFile.AbsolutePath );
			  SetupIndex();

			  LifeSupport life = new LifeSupport();
			  ConsoleInput consoleInput = life.Add( new ConsoleInput( System.in, System.out, staticPrompt( "# " ) ) );
			  consoleInput.Add( "print", new Print( this ) );
			  consoleInput.Add( "add", new AddCommand( this ) );
			  consoleInput.Add( "remove", new RemoveCommand( this ) );
			  consoleInput.Add( "cp", new Checkpoint( this ) );
			  consoleInput.Add( "autoprint", new ToggleAutoPrintCommand( this ) );
			  consoleInput.Add( "restart", new RestartCommand( this ) );
			  consoleInput.Add( "state", new PrintStateCommand( this ) );
			  consoleInput.Add( "cc", new ConsistencyCheckCommand( this ) );

			  life.Start();
			  try
			  {
					consoleInput.WaitFor();
			  }
			  finally
			  {
					life.Shutdown();
			  }
		 }

		 private class Print : Command
		 {
			 private readonly GBPTreePlayground _outerInstance;

			 public Print( GBPTreePlayground outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(String[] args, java.io.PrintStream out) throws Exception
			  public override void Run( string[] args, PrintStream @out )
			  {
					outerInstance.tree.PrintTree();
			  }

			  public override string ToString()
			  {
					return "Print tree";
			  }
		 }

		 private class PrintStateCommand : Command
		 {
			 private readonly GBPTreePlayground _outerInstance;

			 public PrintStateCommand( GBPTreePlayground outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(String[] args, java.io.PrintStream out) throws Exception
			  public override void Run( string[] args, PrintStream @out )
			  {
					outerInstance.tree.PrintState();
			  }

			  public override string ToString()
			  {
					return "Print state of tree";
			  }
		 }

		 private class Checkpoint : Command
		 {
			 private readonly GBPTreePlayground _outerInstance;

			 public Checkpoint( GBPTreePlayground outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(String[] args, java.io.PrintStream out) throws Exception
			  public override void Run( string[] args, PrintStream @out )
			  {
					outerInstance.tree.Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }
			  public override string ToString()
			  {
					return "Checkpoint tree";
			  }
		 }

		 private class AddCommand : Command
		 {
			 private readonly GBPTreePlayground _outerInstance;

			 public AddCommand( GBPTreePlayground outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(String[] args, java.io.PrintStream out) throws Exception
			  public override void Run( string[] args, PrintStream @out )
			  {
					long?[] longValues = new long?[args.Length];
					for ( int i = 0; i < args.Length; i++ )
					{
						 longValues[i] = Convert.ToInt64( args[i] );
					}
					MutableLong key = new MutableLong();
					MutableLong value = new MutableLong();
					try
					{
							using ( Writer<MutableLong, MutableLong> writer = outerInstance.tree.Writer() )
							{
							 foreach ( long? longValue in longValues )
							 {
								  key.Value = longValue;
								  value.Value = longValue;
								  writer.Put( key, value );
							 }
							}
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
					outerInstance.maybePrint();
			  }
			  public override string ToString()
			  {
					return "N [N ...] (add key N)";
			  }
		 }

		 private class RemoveCommand : Command
		 {
			 private readonly GBPTreePlayground _outerInstance;

			 public RemoveCommand( GBPTreePlayground outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(String[] args, java.io.PrintStream out) throws Exception
			  public override void Run( string[] args, PrintStream @out )
			  {
					long?[] longValues = new long?[args.Length];
					for ( int i = 0; i < args.Length; i++ )
					{
						 longValues[i] = Convert.ToInt64( args[i] );
					}
					MutableLong key = new MutableLong();
					try
					{
							using ( Writer<MutableLong, MutableLong> writer = outerInstance.tree.Writer() )
							{
							 foreach ( long? longValue in longValues )
							 {
								  key.Value = longValue;
								  writer.Remove( key );
							 }
							}
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
					outerInstance.maybePrint();
			  }
			  public override string ToString()
			  {
					return "N [N ...] (remove key N)";
			  }
		 }

		 private class ToggleAutoPrintCommand : Command
		 {
			 private readonly GBPTreePlayground _outerInstance;

			 public ToggleAutoPrintCommand( GBPTreePlayground outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Run( string[] args, PrintStream @out )
			  {
					if ( outerInstance.autoPrint.True )
					{
						 outerInstance.autoPrint.setFalse();
					}
					else
					{
						 outerInstance.autoPrint.setTrue();
					}
			  }
			  public override string ToString()
			  {
					return "Toggle auto print after modifications (ON by default)";
			  }
		 }

		 private class RestartCommand : Command
		 {
			 private readonly GBPTreePlayground _outerInstance;

			 public RestartCommand( GBPTreePlayground outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(String[] args, java.io.PrintStream out) throws Exception
			  public override void Run( string[] args, PrintStream @out )
			  {
					Console.WriteLine( "Closing tree..." );
					outerInstance.tree.Dispose();
					Console.WriteLine( "Starting tree..." );
					outerInstance.setupIndex();
					Console.WriteLine( "Tree started!" );
			  }
			  public override string ToString()
			  {
					return "Close and open gbptree. No checkpoint is performed.";
			  }
		 }

		 private class ConsistencyCheckCommand : Command
		 {
			 private readonly GBPTreePlayground _outerInstance;

			 public ConsistencyCheckCommand( GBPTreePlayground outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(String[] args, java.io.PrintStream out) throws Exception
			  public override void Run( string[] args, PrintStream @out )
			  {
					Console.WriteLine( "Checking consistency..." );
					outerInstance.tree.ConsistencyCheck();
					Console.WriteLine( "Consistency check finished!" );
			  }

			  public override string ToString()
			  {
					return "Check consistency of GBPTree";
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void maybePrint() throws java.io.IOException
		 private void MaybePrint()
		 {
			  if ( _autoPrint.Value )
			  {
					Print();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void print() throws java.io.IOException
		 private void Print()
		 {
			  _tree.printTree();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws InterruptedException, java.io.IOException
		 public static void Main( string[] args )
		 {
			  File indexFile;
			  if ( args.Length > 0 )
			  {
					indexFile = new File( args[0] );
			  }
			  else
			  {
					indexFile = new File( "index" );
			  }

			  FileSystemAbstraction fs = new DefaultFileSystemAbstraction();
			  ( new GBPTreePlayground( fs, indexFile ) ).Run();
		 }
	}

}