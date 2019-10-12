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
namespace Neo4Net.Kernel.impl.util
{

	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Neo4Net.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.not;

	public class Converters
	{
		 private Converters()
		 {
		 }

		 public static System.Func<string, T> Mandatory<T>()
		 {
			  return key =>
			  {
				throw new System.ArgumentException( "Missing argument '" + key + "'" );
			  };
		 }

		 public static System.Func<string, T> Optional<T>()
		 {
			  return from => null;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> System.Func<String,T> withDefault(final T defaultValue)
		 public static System.Func<string, T> WithDefault<T>( T defaultValue )
		 {
			  return from => defaultValue;
		 }

		 public static System.Func<string, File> ToFile()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return File::new;
		 }

		 public static System.Func<string, Path> ToPath()
		 {
			  return Paths.get;
		 }

		 public static System.Func<string, string> Identity()
		 {
			  return s => s;
		 }

		 public static readonly IComparer<File> ByFileName = System.Collections.IComparer.comparing( File.getName );

		 public static readonly IComparer<File> ByFileNameWithCleverNumbers = ( o1, o2 ) => NumberAwareStringComparator.Instance.Compare( o1.AbsolutePath, o2.AbsolutePath );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<String,java.io.File[]> regexFiles(final boolean cleverNumberRegexSort)
		 public static System.Func<string, File[]> RegexFiles( bool cleverNumberRegexSort )
		 {
			  return name =>
			  {
				IComparer<File> sorting = cleverNumberRegexSort ? ByFileNameWithCleverNumbers : ByFileName;
				IList<File> files = Validators.MatchingFiles( new File( name ) );
				Files.sort( sorting );
				return Files.toArray( new File[Files.size()] );
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<String,java.io.File[]> toFiles(final String delimiter, final System.Func<String,java.io.File[]> eachFileConverter)
		 public static System.Func<string, File[]> ToFiles( string delimiter, System.Func<string, File[]> eachFileConverter )
		 {
			  return from =>
			  {
				if ( from == null )
				{
					 return new File[0];
				}

				string[] names = from.Split( delimiter );
				IList<File> files = new List<File>();
				foreach ( string name in names )
				{
					 Files.addAll( Arrays.asList( eachFileConverter( name ) ) );
				}
				return Files.toArray( new File[Files.size()] );
			  };
		 }

		 public static System.Func<string, int> ToInt()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return int?::new;
		 }

		 /// <summary>
		 /// Takes a raw address that can have a single port or 2 ports (lower and upper bounds of port range) and
		 /// processes it to a clean separation of host and ports. When only one port is specified, it is in the lower bound.
		 /// The presence of an upper bound implies a range.
		 /// </summary>
		 /// <param name="rawAddress"> the raw address that a user can provide via config or command line </param>
		 /// <returns> the host, lower bound port, and upper bound port </returns>
		 public static OptionalHostnamePort ToOptionalHostnamePortFromRawAddress( string rawAddress )
		 {
			  HostnamePort hostnamePort = new HostnamePort( rawAddress );
			  Optional<string> processedHost = Optional.ofNullable( hostnamePort.Host ).map( str => str.replaceAll( "\\[", "" ) ).map( str => str.replaceAll( "]", "" ) );
			  return new OptionalHostnamePort( processedHost, OptionalFromZeroable( hostnamePort.Ports[0] ), OptionalFromZeroable( hostnamePort.Ports[1] ) );
		 }

		 private static int? OptionalFromZeroable( int port )
		 {
			  return port == 0 ? null : port;
		 }
	}

}