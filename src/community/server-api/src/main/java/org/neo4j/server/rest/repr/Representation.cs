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
namespace Neo4Net.Server.rest.repr
{

	public abstract class Representation
	{
		 // non-inlineable constants
		 public static readonly string Graphdb = RepresentationType.Graphdb.valueName;
		 public static readonly string Index = RepresentationType.Index.valueName;
		 public static readonly string NodeIndexes = RepresentationType.NodeIndexRoot.valueName;
		 public static readonly string RelationshipIndexes = RepresentationType.RelationshipIndexRoot.valueName;
		 public static readonly string Node = RepresentationType.Node.valueName;
		 public static readonly string NodeList = RepresentationType.Node.listName;
		 public static readonly string Relationship = RepresentationType.Relationship.valueName;
		 public static readonly string RelationshipList = RepresentationType.Relationship.listName;
		 public static readonly string Path = RepresentationType.Path.valueName;
		 public static readonly string PathList = RepresentationType.Path.listName;
		 public static readonly string RelationshipType = RepresentationType.RelationshipType.valueName;
		 public static readonly string PropertiesMap = RepresentationType.Properties.valueName;
		 public static readonly string ExtensionsMap = RepresentationType.Plugins.valueName;
		 public static readonly string Extension = RepresentationType.Plugin.valueName;
		 public static readonly string Uri = RepresentationType.Uri.valueName;
		 public static readonly string UriTemplate = RepresentationType.Template.valueName;
		 public static readonly string String = RepresentationType.String.valueName;
		 public static readonly string StringList = RepresentationType.String.listName;
		 public static readonly string Byte = RepresentationType.Byte.valueName;
		 public static readonly string ByteList = RepresentationType.Byte.listName;
		 public static readonly string Character = RepresentationType.Char.valueName;
		 public static readonly string CharacterList = RepresentationType.Char.listName;
		 public static readonly string Short = RepresentationType.Short.valueName;
		 public static readonly string ShortList = RepresentationType.Short.listName;
		 public static readonly string Integer = RepresentationType.Integer.valueName;
		 public static readonly string IntegerList = RepresentationType.Integer.listName;
		 public static readonly string Long = RepresentationType.Long.valueName;
		 public static readonly string LongList = RepresentationType.Long.listName;
		 public static readonly string Float = RepresentationType.Float.valueName;
		 public static readonly string FloatList = RepresentationType.Float.listName;
		 public static readonly string Double = RepresentationType.Double.valueName;
		 public static readonly string DoubleList = RepresentationType.Double.listName;
		 public static readonly string Boolean = RepresentationType.Boolean.valueName;
		 public static readonly string BooleanList = RepresentationType.Boolean.listName;
		 public static readonly string Exception = RepresentationType.Exception.valueName;
		 public static readonly string Map = RepresentationType.Map.valueName;

		 internal readonly RepresentationType Type;

		 internal Representation( RepresentationType type )
		 {
			  this.Type = type;
		 }

		 internal Representation( string type )
		 {
			  this.Type = new RepresentationType( type );
		 }

		 public virtual RepresentationType RepresentationType
		 {
			 get
			 {
				  return this.Type;
			 }
		 }

		 internal abstract string Serialize( RepresentationFormat format, URI baseUri, ExtensionInjector extensions );

		 internal abstract void AddTo( ListSerializer serializer );

		 internal abstract void PutTo( MappingSerializer serializer, string key );

		 internal virtual bool Empty
		 {
			 get
			 {
				  return false;
			 }
		 }

		 public static Representation EmptyRepresentation()
		 {
			  return new RepresentationAnonymousInnerClass();
		 }

		 private class RepresentationAnonymousInnerClass : Representation
		 {
			 public RepresentationAnonymousInnerClass() : base((RepresentationType) null)
			 {
			 }

			 internal override bool Empty
			 {
				 get
				 {
					  return true;
				 }
			 }

			 internal override string serialize( RepresentationFormat format, URI baseUri, ExtensionInjector extensions )
			 {
				  return "";
			 }

			 internal override void putTo( MappingSerializer serializer, string key )
			 {
			 }

			 internal override void addTo( ListSerializer serializer )
			 {
			 }
		 }
	}

}