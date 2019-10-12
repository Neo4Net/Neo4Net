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
namespace Neo4Net.Index.@internal.gbptree
{

	/// <summary>
	/// Able to select implementation of <seealso cref="TreeNode"/> to use in different scenarios, should be used in favor of directly
	/// instantiating <seealso cref="TreeNode"/> instances.
	/// </summary>
	internal class TreeNodeSelector
	{
		 /// <summary>
		 /// Creates <seealso cref="TreeNodeFixedSize"/> instances.
		 /// </summary>
		 internal static Factory FIXED = new FactoryAnonymousInnerClass();

		 private class FactoryAnonymousInnerClass : Factory
		 {
			 public TreeNode<KEY, VALUE> create<KEY, VALUE>( int pageSize, Layout<KEY, VALUE> layout )
			 {
				  return new TreeNodeFixedSize<KEY, VALUE>( pageSize, layout );
			 }

			 public sbyte formatIdentifier()
			 {
				  return TreeNodeFixedSize.FORMAT_IDENTIFIER;
			 }

			 public sbyte formatVersion()
			 {
				  return TreeNodeFixedSize.FORMAT_VERSION;
			 }
		 }

		 /// <summary>
		 /// Creates <seealso cref="TreeNodeDynamicSize"/> instances.
		 /// </summary>
		 internal static Factory DYNAMIC = new FactoryAnonymousInnerClass2();

		 private class FactoryAnonymousInnerClass2 : Factory
		 {
			 public TreeNode<KEY, VALUE> create<KEY, VALUE>( int pageSize, Layout<KEY, VALUE> layout )
			 {
				  return new TreeNodeDynamicSize<KEY, VALUE>( pageSize, layout );
			 }

			 public sbyte formatIdentifier()
			 {
				  return TreeNodeDynamicSize.FORMAT_IDENTIFIER;
			 }

			 public sbyte formatVersion()
			 {
				  return TreeNodeDynamicSize.FORMAT_VERSION;
			 }
		 }

		 /// <summary>
		 /// Selects a format based on the given <seealso cref="Layout"/>.
		 /// </summary>
		 /// <param name="layout"> <seealso cref="Layout"/> dictating which <seealso cref="TreeNode"/> to instantiate. </param>
		 /// <returns> a <seealso cref="Factory"/> capable of instantiating the selected format. </returns>
		 internal static Factory SelectByLayout<T1>( Layout<T1> layout )
		 {
			  // For now the selection is done in a simple fashion, by looking at layout.fixedSize().
			  return layout.FixedSize() ? FIXED : DYNAMIC;
		 }

		 /// <summary>
		 /// Selects a format based on the given format specification.
		 /// </summary>
		 /// <param name="formatIdentifier"> format identifier, see <seealso cref="Meta.getFormatIdentifier()"/> </param>
		 /// <param name="formatVersion"> format version, see <seealso cref="Meta.getFormatVersion()"/>. </param>
		 /// <returns> a <seealso cref="Factory"/> capable of instantiating the selected format. </returns>
		 internal static Factory SelectByFormat( sbyte formatIdentifier, sbyte formatVersion )
		 {
			  // For now do a simple selection of the two formats we know. Moving forward this can contain
			  // many more identifiers and different versions of each.
			  if ( formatIdentifier == TreeNodeFixedSize.FORMAT_IDENTIFIER && formatVersion == TreeNodeFixedSize.FORMAT_VERSION )
			  {
					return FIXED;
			  }
			  else if ( formatIdentifier == TreeNodeDynamicSize.FORMAT_IDENTIFIER && formatVersion == TreeNodeDynamicSize.FORMAT_VERSION )
			  {
					return DYNAMIC;
			  }
			  throw new System.ArgumentException( format( "Unknown format identifier:%d and version:%d combination", formatIdentifier, formatVersion ) );
		 }

		 /// <summary>
		 /// Able to instantiate <seealso cref="TreeNode"/> of a specific format and version.
		 /// </summary>
		 internal interface Factory
		 {
			  /// <summary>
			  /// Instantiates a <seealso cref="TreeNode"/> of a specific format and version that this factory represents.
			  /// </summary>
			  /// <param name="pageSize"> page size, i.e. size of tree nodes. </param>
			  /// <param name="layout"> <seealso cref="Layout"/> that will be used in this format. </param>
			  /// <returns> the instantiated <seealso cref="TreeNode"/>. </returns>
			  TreeNode<KEY, VALUE> create<KEY, VALUE>( int pageSize, Layout<KEY, VALUE> layout );

			  /// <summary>
			  /// Specifies the format identifier of the physical layout of tree nodes.
			  /// A format identifier must be unique among all possible existing format identifiers.
			  /// It's used to differentiate between different types of formats.
			  /// On top of this a specific <seealso cref="formatVersion() format version"/> can specify a version of this format.
			  /// </summary>
			  /// <returns> format identifier for the specific <seealso cref="TreeNode"/> that this factory represents.
			  /// Can return this w/o instantiating the <seealso cref="TreeNode"/>. </returns>
			  sbyte FormatIdentifier();

			  /// <summary>
			  /// Specifies the version of this particular <seealso cref="formatIdentifier() format"/>. It must be unique
			  /// among all other versions of this <seealso cref="formatIdentifier() format"/>.
			  /// </summary>
			  /// <returns> format version for the specific <seealso cref="TreeNode"/> that this factory represents.
			  /// Can return this w/o instantiating the <seealso cref="TreeNode"/>. </returns>
			  sbyte FormatVersion();
		 }
	}

}