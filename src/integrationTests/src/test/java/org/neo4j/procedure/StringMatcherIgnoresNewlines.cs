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
namespace Neo4Net.Procedure
{
	using Matcher = org.hamcrest.Matcher;
	using StringContains = org.hamcrest.core.StringContains;

	public class StringMatcherIgnoresNewlines
	{
		 private StringMatcherIgnoresNewlines()
		 {
		 }

		 public static Matcher<string> ContainsStringIgnoreNewlines( string substring )
		 {
			  return new StringContainsAnonymousInnerClass( substring );
		 }

		 private class StringContainsAnonymousInnerClass : StringContains
		 {
			 private string _substring;

			 public StringContainsAnonymousInnerClass( string substring ) : base( substring )
			 {
				 this._substring = substring;
			 }

			 internal Pattern newLines = Pattern.compile( "\\s*[\\r\\n]+\\s*" );

			 private string clean( string @string )
			 {
				  return newLines.matcher( @string ).replaceAll( "" );
			 }

			 protected internal override bool evalSubstringOf( string s )
			 {
				  return clean( s ).contains( clean( _substring ) );
			 }
		 }
	}

}