using System.Collections;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.backup
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using BackupClient = Neo4Net.backup.impl.BackupClient;
	using BackupProtocolService = Neo4Net.backup.impl.BackupProtocolService;
	using ConsistencyCheck = Neo4Net.backup.impl.ConsistencyCheck;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// This test builds all valid combinations/permutations of args for <seealso cref="org.Neo4Net.backup.BackupTool"/> and asserts
	/// that it can handle those.
	/// It tests legacy and modern sets of args in all possible forms: (-option, --option, -option value, -option=value).
	/// Legacy is (-from, -to, -verify) and modern is (-host, -port, -to, -verify).
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BackupToolCmdArgumentsAcceptanceTest
	public class BackupToolCmdArgumentsAcceptanceTest
	{
		 private const string HOST = "localhost";
		 private const int PORT = 9090;
		 private static readonly Path _path = Paths.get( "/var/backup/Neo4Net/" );

		 [Parameter(0)]
		 public string ArgsAsString;
		 [Parameter(1)]
		 public bool ExpectedVerifyStoreValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "args=({0})") public static Iterable<Object[]> data()
		 public static IEnumerable<object[]> Data()
		 {
			  return Iterables.concat( AllCombinations( stringMap( "host", HOST, "port", PORT.ToString(), "to", _path.ToString() ) ), AllCombinations(stringMap("from", HOST + ":" + PORT, "to", _path.ToString())) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeBackupServiceWhenArgsAreValid() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeBackupServiceWhenArgsAreValid()
		 {
			  // Given
			  string[] args = ArgsAsString.Split( " ", true );

			  BackupProtocolService backupProtocolService = mock( typeof( BackupProtocolService ) );
			  PrintStream printStream = mock( typeof( PrintStream ) );
			  BackupTool backupTool = new BackupTool( backupProtocolService, printStream );

			  // When
			  backupTool.Run( args );

			  // Then
			  verify( backupProtocolService ).doIncrementalBackupOrFallbackToFull( eq( HOST ), eq( PORT ), eq( DatabaseLayout.of( _path.toFile() ) ), ExpectedVerifyStoreValue ? eq(ConsistencyCheck.FULL) : eq(ConsistencyCheck.NONE), any(typeof(Config)), eq(BackupClient.BIG_READ_TIMEOUT), eq(false) );
		 }

		 private static IList<string> AllFlagValues( string name, bool value )
		 {
			  return value ? new IList<string> { "-" + name, "--" + name, "-" + name + "=true", "--" + name + "=true", "-" + name + " true", "--" + name + " true" } : new IList<string> { "-" + name + "=false", "--" + name + "=false", "-" + name + " false", "--" + name + " false" };
		 }

		 private static IEnumerable<object[]> AllCombinations( IDictionary<string, string> optionsMap )
		 {
			  return Iterables.concat( AllCombinations( optionsMap, true ), AllCombinations( optionsMap, false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static java.util.List<Object[]> allCombinations(java.util.Map<String,String> optionsMap, boolean verifyStore)
		 private static IList<object[]> AllCombinations( IDictionary<string, string> optionsMap, bool verifyStore )
		 {
			  IList<object[]> result = new List<object[]>();

			  IList<IList<string>> optionCombinations = GatherAllOptionCombinations( optionsMap );
			  IList<string> verifyFlagValues = AllFlagValues( "verify", verifyStore );

			  foreach ( IList<string> options in optionCombinations )
			  {
					foreach ( string flag in verifyFlagValues )
					{
						 IList<string> args = Join( options, flag );

						 StringBuilder sb = new StringBuilder();
						 foreach ( string arg in args )
						 {
							  sb.Append( sb.Length > 0 ? " " : "" ).Append( arg );
						 }
						 string argsJoinedAsString = sb.ToString();
						 result.Add( new object[]{ argsJoinedAsString, verifyStore } );
					}
			  }

			  return result;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static java.util.List<java.util.List<String>> gatherAllOptionCombinations(java.util.Map<String,String> optionsMap)
		 private static IList<IList<string>> GatherAllOptionCombinations( IDictionary<string, string> optionsMap )
		 {
			  IList<IList<string>> result = new List<IList<string>>();

			  Deque<string> stack = new LinkedList<string>();
			  KeyValuePair<string, string>[] entries = optionsMap.SetOfKeyValuePairs().toArray(new DictionaryEntry[0]);
			  GatherAllOptionCombinations( entries, 0, stack, result );

			  return result;
		 }

		 private static void GatherAllOptionCombinations( KeyValuePair<string, string>[] entries, int current, Deque<string> stack, IList<IList<string>> result )
		 {
			  if ( current == entries.Length )
			  {
					result.Add( new List<>( stack ) );
			  }
			  else
			  {
					KeyValuePair<string, string> entry = entries[current];
					int next = current + 1;

					foreach ( string arg in PossibleArgs( entry.Key, entry.Value ) )
					{
						 stack.push( arg );
						 GatherAllOptionCombinations( entries, next, stack, result );
						 stack.pop();
					}
			  }
		 }

		 private static IList<string> PossibleArgs( string key, string value )
		 {
			  return ( string.ReferenceEquals( value, null ) ) ? new IList<string> { "-" + key, "--" + key } : new IList<string> { "-" + key + "=" + value, "--" + key + "=" + value, "-" + key + " " + value, "--" + key + " " + value };
		 }

		 private static IList<string> Join( IList<string> list, string element )
		 {
			  IList<string> result = new List<string>( list );
			  result.Add( element );
			  return result;
		 }
	}

}