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
namespace Neo4Net.CommandLine.Admin
{

	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using Service = Neo4Net.Helpers.Service;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

	/// <summary>
	/// To create a command for {@code neo4j-admin}:
	/// <ol>
	///   <li>implement {@code AdminCommand}</li>
	///   <li>create a concrete subclass of {@code AdminCommand.Provider} which instantiates the command</li>
	///   <li>register the {@code Provider} in {@code META-INF/services} as described
	///     <a href='https://docs.oracle.com/javase/8/docs/api/java/util/ServiceLoader.html'>here</a></li>
	/// </ol>
	/// </summary>
	public interface AdminCommand
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void execute(String[] args) throws IncorrectUsage, CommandFailed;
		 void Execute( string[] args );
	}

	 public abstract class AdminCommand_Provider : Service
	 {
		  /// <summary>
		  /// Create a new instance of a service implementation identified with the
		  /// specified key(s).
		  /// </summary>
		  /// <param name="key">     the main key for identifying this service implementation </param>
		  /// <param name="altKeys"> alternative spellings of the identifier of this service </param>
		  protected internal AdminCommand_Provider( string key, params string[] altKeys ) : base( key, altKeys )
		  {
		  }

		  /// <returns> The command's name </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public String name()
		  public virtual string Name()
		  {
				return Iterables.last( Keys );
		  }

		  /// <returns> The arguments this command accepts. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public abstract org.neo4j.commandline.arguments.Arguments allArguments();
		  public abstract Arguments AllArguments();

		  /// 
		  /// <returns> A list of possibly mutually-exclusive argument sets for this command. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public java.util.List<org.neo4j.commandline.arguments.Arguments> possibleArguments()
		  public virtual IList<Arguments> PossibleArguments()
		  {
				return Collections.singletonList( AllArguments() );
		  }

		  /// <returns> A single-line summary for the command. Should be 70 characters or less. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public abstract String summary();
		  public abstract string Summary();

		  /// <returns> AdminCommandSection the command using the provider is grouped under </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public abstract AdminCommandSection commandSection();
		  public abstract AdminCommandSection CommandSection();

		  /// <returns> A description for the command's help text. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public abstract String description();
		  public abstract string Description();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull public abstract AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, OutsideWorld outsideWorld);
		  public abstract AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld );

		  public void PrintSummary( System.Action<string> output )
		  {
				output( string.Format( "{0}", Name() ) );
				output( "    " + Summary() );
		  }
	 }

	 public interface AdminCommand_Blocker
	 {
		  /// <param name="homeDir">   the home of the Neo4j installation. </param>
		  /// <param name="configDir"> the directory where configuration files can be found. </param>
		  /// <returns> A boolean representing whether or not this command should be blocked from running. </returns>
		  bool DoesBlock( Path homeDir, Path configDir );

		  /// <returns> A list of the commands this blocker applies to. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull Set<String> commands();
		  ISet<string> Commands();

		  /// <returns> An explanation of why a command was blocked. This will be shown to the user. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull String explanation();
		  string Explanation();
	 }

}