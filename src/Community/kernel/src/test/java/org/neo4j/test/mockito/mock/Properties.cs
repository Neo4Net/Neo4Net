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
namespace Neo4Net.Test.mockito.mock
{
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using NotFoundException = Neo4Net.Graphdb.NotFoundException;

	public class Properties : Answer<object>, IEnumerable<string>
	{
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Properties PropertiesConflict( params Property[] properties )
		 {
			  return new Properties( properties );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Properties PropertiesConflict( IDictionary<string, object> properties )
		 {
			  return new Properties( properties );
		 }

		 private readonly SortedDictionary<string, object> _properties = new SortedDictionary<string, object>();

		 private Properties( Property[] properties )
		 {
			  foreach ( Property property in properties )
			  {
					this._properties[property.Key()] = property.Value();
			  }
		 }

		 private Properties( IDictionary<string, object> properties )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  this._properties.putAll( properties );
		 }

		 public override object Answer( InvocationOnMock invocation )
		 {
			  object[] arguments = invocation.Arguments;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SuspiciousMethodCalls") Object result = properties.get(arguments[0]);
			  object result = _properties[arguments[0]];
			  if ( result == null )
			  {
					if ( arguments.Length == 2 )
					{
						 return arguments[1];
					}
					else
					{
						 throw new NotFoundException();
					}
			  }
			  return result;
		 }

		 public override IEnumerator<string> Iterator()
		 {
			  return _properties.Keys.GetEnumerator();
		 }

		 public virtual SortedDictionary<string, object> GetProperties()
		 {
			  return _properties;
		 }
	}

}