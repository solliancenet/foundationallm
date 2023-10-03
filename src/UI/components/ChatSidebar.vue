<template>
	<h1>ChatSidebar</h1>
</template>

<script lang="ts">

interface Chat {

}

export default {
  name: 'ChatSidebar',

  data () {
    return {
    	chats: [
    		{ title: 'Chat 1' },
    		{ title: 'Chat 2' },
    	]
    }
  }
}
</script>

<style lang="scss" scoped>

	.sidebar {

	}

</style>