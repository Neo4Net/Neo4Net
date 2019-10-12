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
namespace Org.Neo4j.Server.rest.transactional
{

	public abstract class ResultDataContent
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       row { public ResultDataContentWriter writer(java.net.URI baseUri) { return new RowWriter(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       graph { public ResultDataContentWriter writer(java.net.URI baseUri) { return new GraphExtractionWriter(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       rest { public ResultDataContentWriter writer(java.net.URI baseUri) { return new RestRepresentationWriter(baseUri); } };

		 private static readonly IList<ResultDataContent> valueList = new List<ResultDataContent>();

		 static ResultDataContent()
		 {
			 valueList.Add( row );
			 valueList.Add( graph );
			 valueList.Add( rest );
		 }

		 public enum InnerEnum
		 {
			 row,
			 graph,
			 rest
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private ResultDataContent( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public abstract ResultDataContentWriter writer( java.net.URI baseUri );

		 public static readonly ResultDataContent public static ResultDataContent[] fromNames( java.util.List<JavaToDotNetGenericWildcard> names )
		 {
			 if ( names == null || names.isEmpty() ) { return null; } ResultDataContent[] result = new ResultDataContent[names.size()]; java.util.Iterator<JavaToDotNetGenericWildcard> name = names.iterator(); for (int i = 0; i < result.length; i++)
			 {
				 Object contentName = name.next(); if (contentName instanceof String)
				 {
					 try { result[i] = valueOf( ( ( String ) contentName ).toLowerCase() ); } catch (IllegalArgumentException e) { throw new IllegalArgumentException("Invalid result data content specifier: " + contentName); }
				 }
				 else { throw new IllegalArgumentException( "Invalid result data content specifier: " + contentName ); }
			 }
			 return result;
		 }
		 = new ResultDataContent("public static ResultDataContent[] fromNames(java.util.List<JavaToDotNetGenericWildcard> names) { if(names == null || names.isEmpty()) { return null; } ResultDataContent[] result = new ResultDataContent[names.size()]; java.util.Iterator<JavaToDotNetGenericWildcard> name = names.iterator(); for(int i = 0; i < result.length; i++) { Object contentName = name.next(); if(contentName instanceof String) { try { result[i] = valueOf(((String) contentName).toLowerCase()); } catch(IllegalArgumentException e) { throw new IllegalArgumentException("Invalid result data content specifier: " + contentName); } } else { throw new IllegalArgumentException("Invalid result data content specifier: " + contentName); } } return result; }", InnerEnum.public static ResultDataContent[] fromNames(java.util.List<JavaToDotNetGenericWildcard> names)
		 {
			 if ( names == null || names.isEmpty() ) { return null; } ResultDataContent[] result = new ResultDataContent[names.size()]; java.util.Iterator<JavaToDotNetGenericWildcard> name = names.iterator(); for (int i = 0; i < result.length; i++)
			 {
				 Object contentName = name.next(); if (contentName instanceof String)
				 {
					 try { result[i] = valueOf( ( ( String ) contentName ).toLowerCase() ); } catch (IllegalArgumentException e) { throw new IllegalArgumentException("Invalid result data content specifier: " + contentName); }
				 }
				 else { throw new IllegalArgumentException( "Invalid result data content specifier: " + contentName ); }
			 }
			 return result;
		 });

		public static IList<ResultDataContent> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static ResultDataContent valueOf( string name )
		{
			foreach ( ResultDataContent enumInstance in ResultDataContent.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}