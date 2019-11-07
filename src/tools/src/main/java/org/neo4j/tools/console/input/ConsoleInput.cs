using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
namespace Neo4Net.tools.console.input
{

	using Neo4Net.Kernel.impl.util;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.tools.console.input.ConsoleUtil.tokenizeStringWithQuotes;

	/// <summary>
	/// Useful utility which listens to input from console and reacts to each line, i.e. after each ENTER.
	/// <seealso cref="Command"/> are added with <seealso cref="add(string, Command)"/> and are then reacted to in a separate thread,
	/// which continuously sits and listens to console input.
	/// 
	/// Use of this class can be a shell-like tool which boots up, instantiates a <seealso cref="ConsoleInput"/>,
	/// <seealso cref="start() starts it"/> followed by <seealso cref="waitFor()"/> which will block until the input stream ends
	/// or an exit command is issued.
	/// 
	/// Another use is to instantiate <seealso cref="ConsoleInput"/>, <seealso cref="start() start it"/> and then move on to do
	/// something else entirely. That way the commands added here will be available user input something in
	/// the console while all other things are happening. In this case <seealso cref="shutdown()"/> should be called
	/// when the application otherwise shuts down.
	/// </summary>
	public class ConsoleInput : LifecycleAdapter
	{
		 private readonly IDictionary<string, Command> _commands = new Dictionary<string, Command>();
		 private Reactor _reactor;
		 private readonly StreamReader _inputReader;
		 private readonly Listener<PrintStream> _prompt;
		 private readonly PrintStream @out;

		 public ConsoleInput( Stream input, PrintStream @out, Listener<PrintStream> prompt )
		 {
			  this.@out = @out;
			  this._prompt = prompt;
			  this._inputReader = new StreamReader( input );
		 }

		 /// <summary>
		 /// Add <seealso cref="Command"/> to be available and executed when input uses it.
		 /// </summary>
		 /// <param name="name"> command name, i.e the first word of the whole command line to listen for. </param>
		 /// <param name="command"> <seealso cref="Command"/> to <seealso cref="Command.run(string[], PrintStream) run"/> as part of command line
		 /// starting with {@code name}- </param>
		 public virtual void Add( string name, Command command )
		 {
			  _commands[name] = command;
		 }

		 /// <summary>
		 /// Starts to listen on the input supplied in constructor.
		 /// </summary>
		 public override void Start()
		 {
			  _reactor = new Reactor( this );
			  _reactor.Start();
		 }

		 /// <summary>
		 /// Waits till input stream ends or exit command is given.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void waitFor() throws InterruptedException
		 public virtual void WaitFor()
		 {
			  _reactor.Join();
		 }

		 /// <summary>
		 /// Shuts down and stops listen on the input.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws InterruptedException
		 public override void Stop()
		 {
			  _reactor.halt();
			  WaitFor();
		 }

		 /// <summary>
		 /// Prints usage and help for all available commands.
		 /// </summary>
		 public virtual void PrintUsage()
		 {
			  @out.println( "Available commands:" );
			  foreach ( KeyValuePair<string, Command> entry in _commands.SetOfKeyValuePairs() )
			  {
					@out.println( entry.Key + ": " + entry.Value );
			  }
		 }

		 private class Reactor : Thread
		 {
			 private readonly ConsoleInput _outerInstance;

			  internal volatile bool Halted;

			  internal Reactor( ConsoleInput outerInstance ) : base( typeof( ConsoleInput ).Name + " reactor" )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal virtual void Halt()
			  {
					Halted = true;

					// Interrupt this thread since it's probably sitting listening to input.
					interrupt();
			  }

			  public override void Run()
			  {
					while ( !Halted )
					{
						 try
						 {
							  outerInstance.prompt.Receive( outerInstance.@out );
							  string commandLine = outerInstance.inputReader.ReadLine(); // Blocking call
							  if ( string.ReferenceEquals( commandLine, null ) )
							  {
									Halted = true;
									break;
							  }

							  string[] args = tokenizeStringWithQuotes( commandLine );
							  if ( args.Length == 0 )
							  {
									continue;
							  }
							  string commandName = args[0];
							  Command action = outerInstance.commands[commandName];
							  if ( action != null )
							  {
									action.Run( copyOfRange( args, 1, args.Length ), outerInstance.@out );
							  }
							  else
							  {
									switch ( commandName )
									{
										 case "help":
										 case "?":
										 case "man":
											  outerInstance.PrintUsage();
											  break;
										 case "exit":
											  Halt();
											  break;
										 default:
											  Console.Error.WriteLine( "Unrecognized command '" + commandName + "'" );
											  break;
									}
							  }
						 }
						 catch ( Exception e )
						 {
							  Console.WriteLine( e.ToString() );
							  Console.Write( e.StackTrace );
							  // The show must go on
						 }
					}
			  }
		 }
	}

}