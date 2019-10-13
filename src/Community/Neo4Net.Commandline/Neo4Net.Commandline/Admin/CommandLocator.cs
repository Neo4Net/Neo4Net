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
namespace Neo4Net.CommandLine.Admin
{

	using Service = Neo4Net.Helpers.Service;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;

	/// <summary>
	/// The CommandLocator locates named commands for the AdminTool, or supplies the set of available commands for printing
	/// help output.
	/// </summary>
	public interface CommandLocator
	{
		 /// <summary>
		 /// Find a command provider that matches the given key or name, or throws <seealso cref="NoSuchElementException"/> if no
		 /// matching provider was found. </summary>
		 /// <param name="name"> The name of the provider to look for. </param>
		 /// <returns> Any matching command provider. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: AdminCommand_Provider findProvider(String name) throws java.util.NoSuchElementException;
		 AdminCommand_Provider FindProvider( string name );

		 /// <summary>
		 /// Get an iterable of all of the command providers that are available through this command locator instance. </summary>
		 /// <returns> An iterable of command providers. </returns>
		 IEnumerable<AdminCommand_Provider> AllProviders { get; }

		 /// <summary>
		 /// Get a command locator that uses the <seealso cref="Service service locator"/> mechanism to find providers by their service
		 /// key. </summary>
		 /// <returns> A service locator based command locator. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static CommandLocator fromServiceLocator()
	//	 {
	//		  return new CommandLocator()
	//		  {
	//				@@Override public AdminCommand.Provider findProvider(String name)
	//				{
	//					 return Service.load(AdminCommand.Provider.class, name);
	//				}
	//
	//				@@Override public Iterable<AdminCommand.Provider> getAllProviders()
	//				{
	//					 return Service.load(AdminCommand.Provider.class);
	//				}
	//		  };
	//	 }

		 /// <summary>
		 /// Augment the given command locator such that it also considers the command provider given through the supplier. </summary>
		 /// <param name="command"> A supplier of an additional command. Note that this may be called multiple times. </param>
		 /// <param name="commands"> The command locator to augment with the additional command provider. </param>
		 /// <returns> The augmented command locator. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static CommandLocator withAdditionalCommand(System.Func<AdminCommand_Provider> command, CommandLocator commands)
	//	 {
	//		  return new CommandLocator()
	//		  {
	//				@@Override public AdminCommand.Provider findProvider(String name)
	//				{
	//					 AdminCommand.Provider provider = command.get();
	//					 return Objects.equals(name, provider.name()) ? provider : commands.findProvider(name);
	//				}
	//
	//				@@Override public Iterable<AdminCommand.Provider> getAllProviders()
	//				{
	//					 return Iterables.append(command.get(), commands.getAllProviders());
	//				}
	//		  };
	//	 }
	}

}