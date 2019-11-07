using System.Collections;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.CommandLine.Args
{
	using WordUtils = org.apache.commons.text.WordUtils;


	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Database = Neo4Net.CommandLine.Args.Common.Database;
	using MandatoryCanonicalPath = Neo4Net.CommandLine.Args.Common.MandatoryCanonicalPath;
	using OptionalCanonicalPath = Neo4Net.CommandLine.Args.Common.OptionalCanonicalPath;
	using Args = Neo4Net.Helpers.Args;

	/// <summary>
	/// Builder class for constructing a suitable arguments-string for displaying in help messages and alike.
	/// Some common arguments have convenience functions.
	/// </summary>
	public class Arguments
	{
		 public static readonly Arguments NoArgs = new Arguments();
		 private const int LINE_LENGTH = 80;
		 private const int MIN_RIGHT_COL_WIDTH = 30;
		 private static readonly string _newline = System.getProperty( "line.separator" );
		 private readonly IDictionary<string, NamedArgument> _namedArgs;
		 private readonly List<PositionalArgument> _positionalArgs;
		 private Args _parsedArgs;

		 public Arguments()
		 {
			  _namedArgs = new LinkedHashMap<string, NamedArgument>();
			  _positionalArgs = new List<PositionalArgument>();
		 }

		 public virtual Arguments WithDatabase()
		 {
			  return WithArgument( new Database() );
		 }

		 public virtual Arguments WithDatabase( string description )
		 {
			  return WithArgument( new Database( description ) );
		 }

		 public virtual Arguments WithAdditionalConfig()
		 {
			  return WithArgument( new OptionalCanonicalPath( "additional-config", "config-file-path", "", "Configuration file to supply additional configuration in." ) );
		 }

		 public virtual Arguments WithTo( string description )
		 {
			  return WithArgument( new MandatoryCanonicalPath( "to", "destination-path", description ) );
		 }

		 public virtual Arguments WithOptionalPositionalArgument( int position, string value )
		 {
			  return WithPositionalArgument( new OptionalPositionalArgument( position, value ) );
		 }

		 public virtual Arguments WithMandatoryPositionalArgument( int position, string value )
		 {
			  return WithPositionalArgument( new MandatoryPositionalArgument( position, value ) );
		 }

		 public virtual Arguments WithArgument( NamedArgument namedArgument )
		 {
			  _namedArgs[namedArgument.Name()] = namedArgument;
			  return this;
		 }

		 public virtual Arguments WithPositionalArgument( PositionalArgument arg )
		 {
			  _positionalArgs.Add( arg );
			  return this;
		 }

		 public virtual string Usage()
		 {
			  StringBuilder sb = new StringBuilder();

			  if ( _namedArgs.Count > 0 )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					sb.Append( _namedArgs.Values.Select( NamedArgument::usage ).collect( Collectors.joining( " " ) ) );
			  }

			  if ( _positionalArgs.Count > 0 )
			  {
					sb.Append( " " );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					_positionalArgs.sort( System.Collections.IComparer.comparingInt( PositionalArgument::position ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					sb.Append( _positionalArgs.Select( PositionalArgument::usage ).collect( Collectors.joining( " " ) ) );
			  }

			  return sb.ToString().Trim();
		 }

		 public virtual string Description( string text )
		 {
			  string wrappedText = WrapText( text, LINE_LENGTH );
			  if ( _namedArgs.Count == 0 )
			  {
					return wrappedText;
			  }

			  wrappedText = string.join( _newline + _newline, wrappedText, "options:" );

			  //noinspection OptionalGetWithoutIsPresent handled by if-statement above
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int alignLength = namedArgs.values().stream().map(a -> a.optionsListing().length()).reduce(0, System.Nullable<int>::max);
			  int alignLength = _namedArgs.Values.Select( a => a.optionsListing().length() ).Aggregate(0, int?.max);

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return string.join( _newline, wrappedText, _namedArgs.Values.Select( c => FormatArgumentDescription( alignLength, c ) ).collect( Collectors.joining( _newline ) ) );
		 }

		 /// <summary>
		 /// Original line-endings in the text are respected.
		 /// </summary>
		 /// <param name="text"> to wrap </param>
		 /// <param name="lineLength"> no line will exceed this length </param>
		 /// <returns> the text where no line exceeds the specified length </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static String wrapText(final String text, final int lineLength)
		 public static string WrapText( string text, int lineLength )
		 {
			  IList<string> lines = Arrays.asList( text.Split( "\r?\n", true ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return lines.Select( l => l.length() > lineLength ? WordUtils.wrap(l, lineLength) : l ).collect(Collectors.joining(_newline));
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public String formatArgumentDescription(final int longestAlignmentLength, final NamedArgument argument)
		 public virtual string FormatArgumentDescription( int longestAlignmentLength, NamedArgument argument )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String left = format("  %s", argument.optionsListing());
			  string left = format( "  %s", argument.OptionsListing() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String right;
			  string right;
			  if ( argument is OptionalNamedArg )
			  {
					right = format( "%s [default:%s]", argument.Description(), ((OptionalNamedArg) argument).DefaultValue() );
			  }
			  else
			  {
					right = argument.Description();
			  }
			  // 5 = 2 leading spaces in left + 3 spaces as distance between columns
			  return RightColumnFormatted( left, right, longestAlignmentLength + 5 );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static String rightColumnFormatted(final String leftText, final String rightText, int rightAlignIndex)
		 public static string RightColumnFormatted( string leftText, string rightText, int rightAlignIndex )
		 {
			  const int newLineIndent = 6;
			  int rightWidth = Arguments.LINE_LENGTH - rightAlignIndex;
			  bool startOnNewLine = false;
			  if ( rightWidth < MIN_RIGHT_COL_WIDTH )
			  {
					startOnNewLine = true;
					rightWidth = LINE_LENGTH - newLineIndent;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] rightLines = wrapText(rightText, rightWidth).split(NEWLINE);
			  string[] rightLines = WrapText( rightText, rightWidth ).Split( _newline, true );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String fmt = "%-" + (startOnNewLine ? newLineIndent : rightAlignIndex) + "s%s";
			  string fmt = "%-" + ( startOnNewLine ? newLineIndent : rightAlignIndex ) + "s%s";
			  string firstLine = format( fmt, leftText, startOnNewLine ? "" : rightLines[0] );

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  string rest = java.util.rightLines.Skip( startOnNewLine ? 0 : 1 ).Select( l => format( fmt, "", l ) ).collect( Collectors.joining( _newline ) );

			  if ( rest.Length == 0 )
			  {
					return firstLine;
			  }
			  else
			  {
					return string.join( _newline, firstLine, rest );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Arguments parse(String[] args) throws Neo4Net.commandline.admin.IncorrectUsage
		 public virtual Arguments Parse( string[] args )
		 {
			  // Get boolean flags
			  IList<string> flags = _namedArgs.SetOfKeyValuePairs().Where(e => e.Value is OptionalBooleanArg).Select(DictionaryEntry.getKey).ToList();

			  _parsedArgs = Args.withFlags( flags.ToArray() ).parse(args);
			  Validate();
			  return this;
		 }

		 public virtual string Get( int pos )
		 {
			  if ( pos >= 0 && pos < _positionalArgs.Count )
			  {
					return _positionalArgs[pos].parse( _parsedArgs );
			  }
			  throw new System.ArgumentException( format( "Positional argument '%d' not specified.", pos ) );
		 }

		 public virtual string Get( string argName )
		 {
			  if ( _namedArgs.ContainsKey( argName ) )
			  {
					return _namedArgs[argName].parse( _parsedArgs );
			  }
			  throw new System.ArgumentException( "No such argument available to be parsed: " + argName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validate() throws Neo4Net.commandline.admin.IncorrectUsage
		 private void Validate()
		 {
			  foreach ( string o in _parsedArgs.asMap().Keys )
			  {
					if ( !_namedArgs.ContainsKey( o ) )
					{
						 throw new IncorrectUsage( format( "unrecognized option: '%s'", o ) );
					}
			  }
			  long mandatoryPositionalArgs = _positionalArgs.Where( o => o is MandatoryPositionalArgument ).Count();
			  if ( _parsedArgs.orphans().Count < mandatoryPositionalArgs )
			  {
					throw new IncorrectUsage( "not enough arguments" );
			  }
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  string excessArgs = _parsedArgs.orphans().Skip(_positionalArgs.Count).collect(Collectors.joining(" "));
			  if ( excessArgs.Length > 0 )
			  {
					throw new IncorrectUsage( format( "unrecognized arguments: '%s'", excessArgs ) );
			  }
		 }

		 public virtual bool GetBoolean( string argName )
		 {
			  return Get( argName, bool?.parseBoolean );
		 }

		 public virtual Optional<Path> GetOptionalPath( string argName )
		 {
			  string p = Get( argName );

			  if ( p.Length == 0 )
			  {
					return null;
			  }

			  return Paths.get( p );
		 }

		 public virtual Path GetMandatoryPath( string argName )
		 {
			  Optional<Path> p = GetOptionalPath( argName );
			  if ( p.Present )
			  {
					return p.get();
			  }
			  throw new System.ArgumentException( format( "Missing exampleValue for '%s'", argName ) );
		 }

		 public virtual T Get<T>( string argName, System.Func<string, T> converter )
		 {
			  return converter( Get( argName ) );
		 }

		 /// <param name="argName"> name of argument </param>
		 /// <returns> true if argName was given as an explicit argument, false otherwise </returns>
		 public virtual bool Has( string argName )
		 {
			  if ( _namedArgs.ContainsKey( argName ) )
			  {
					return _namedArgs[argName].has( _parsedArgs );
			  }
			  throw new System.ArgumentException( "No such argument available: " + argName );
		 }
	}

}